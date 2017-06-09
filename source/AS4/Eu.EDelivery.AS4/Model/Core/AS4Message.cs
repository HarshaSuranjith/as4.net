﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Serialization;
using MimeKit;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Internal AS4 Message between MSH
    /// </summary>
    public class AS4Message : IMessage, IEquatable<AS4Message>
    {
        private static readonly AS4Message EmptyMessage = new AS4Message(serializeAsMultiHop: false);
        private readonly bool _serializeAsMultiHop;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Message"/> class.
        /// </summary>
        /// <param name="serializeAsMultiHop">if set to <c>true</c> [serialize as multi hop].</param>
        private AS4Message(bool serializeAsMultiHop = false)
        {
            _serializeAsMultiHop = serializeAsMultiHop;
            ContentType = "application/soap+xml";
            SigningId = new SigningId();
            SecurityHeader = new SecurityHeader();
            Attachments = new List<Attachment>();
            SignalMessages = new List<SignalMessage>();
            UserMessages = new List<UserMessage>();
        }

        public static AS4Message Empty => EmptyMessage;

        public string ContentType { get; set; }

        public XmlDocument EnvelopeDocument { get; set; }

        private bool? __hasMultiHopAttribute;

        /// <summary>
        /// Gets a value indicating whether or not this AS4 Message is a MultiHop message.
        /// </summary>
        public bool IsMultiHopMessage
        {
            get
            {
                if (IsUserMessage && __hasMultiHopAttribute.HasValue == false)
                {
                    __hasMultiHopAttribute = IsMultiHopAttributePresent();
                }

                return (__hasMultiHopAttribute ?? false) || PrimarySignalMessage?.MultiHopRouting != null || _serializeAsMultiHop;
            }
        }

        private bool? IsMultiHopAttributePresent()
        {
            var messagingNode =
                EnvelopeDocument?.SelectSingleNode(
                    "/*[local-name()='Envelope']/*[local-name()='Header']/*[local-name()='Messaging']") as XmlElement;

            if (messagingNode == null)
            {
                return null;
            }

            string role = messagingNode.GetAttribute("role", Constants.Namespaces.Soap12);

            return !string.IsNullOrWhiteSpace(role) && role.Equals(Constants.Namespaces.EbmsNextMsh);
        }

        public ICollection<UserMessage> UserMessages { get; internal set; }

        public ICollection<SignalMessage> SignalMessages { get; internal set; }

        public ICollection<Attachment> Attachments { get; internal set; }

        public SigningId SigningId { get; set; }

        public SecurityHeader SecurityHeader { get; set; }

        public string[] MessageIds
            => UserMessages.Select(m => m.MessageId).Concat(SignalMessages.Select(m => m.MessageId)).ToArray();

        public UserMessage PrimaryUserMessage => UserMessages.FirstOrDefault();

        public SignalMessage PrimarySignalMessage => SignalMessages.FirstOrDefault();

        public bool IsSignalMessage => SignalMessages.Count > 0;

        public bool IsUserMessage => UserMessages.Count > 0;

        public bool IsSigned => SecurityHeader.IsSigned;

        public bool IsEncrypted => SecurityHeader.IsEncrypted;

        public bool HasAttachments => Attachments?.Count != 0;

        public bool IsEmpty => PrimarySignalMessage == null && PrimaryUserMessage == null;

        public bool IsPulling => PrimarySignalMessage is PullRequest;

        /// <summary>
        /// Create message with SOAP envelope.
        /// </summary>
        /// <param name="soapEnvelope">The SOAP envelope.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        public static AS4Message Create(XmlDocument soapEnvelope, string contentType)
        {
            return new AS4Message {EnvelopeDocument = soapEnvelope, ContentType = contentType};
        }

        /// <summary>
        /// Fors the sending p mode.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4Message Create(PMode.SendingProcessingMode pmode)
        {
            return new AS4Message(pmode?.MessagePackaging?.IsMultiHop == true);
        }

        /// <summary>
        /// Gets the primary message identifier.
        /// </summary>
        /// <returns></returns>
        public string GetPrimaryMessageId()
        {
            return IsUserMessage ? PrimaryUserMessage.MessageId : PrimarySignalMessage?.MessageId;
        }

        /// <summary>
        /// Determines the size of the message.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public long DetermineMessageSize(ISerializerProvider provider)
        {
            ISerializer serializer = provider.Get(this.ContentType);

            using (var stream = new DetermineSizeStream())
            {
                serializer.Serialize(this, stream, CancellationToken.None);

                return stream.Length;
            }
        }

        /// <summary>
        /// Add Attachment to <see cref="AS4Message" />
        /// </summary>
        /// <param name="attachment"></param>
        public void AddAttachment(Attachment attachment)
        {
            Attachments.Add(attachment);
            UpdateContentTypeHeader();
        }

        private void UpdateContentTypeHeader()
        {
            string contentTypeString = Constants.ContentTypes.Soap;
            if (Attachments.Count > 0)
            {
                ContentType contentType = new Multipart("related").ContentType;
                contentType.Parameters["type"] = contentTypeString;
                contentType.Charset = Encoding.UTF8.HeaderName.ToLowerInvariant();
                contentTypeString = contentType.ToString();
            }

            ContentType = contentTypeString.Replace("Content-Type: ", string.Empty);
        }

        /// <summary>
        /// Closes the attachments.
        /// </summary>
        public void CloseAttachments()
        {
            foreach (Attachment attachment in Attachments)
            {
                attachment.Content.Dispose();
            }
        }

        /// <summary>
        /// Adds the attachments.
        /// </summary>
        /// <param name="payloads">The payloads.</param>
        /// <param name="retrieval">The retrieval.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task AddAttachments(IReadOnlyList<Payload> payloads, Func<Payload, Task<Stream>> retrieval)
        {
            foreach (Payload payload in payloads)
            {
                Attachment attachment = CreateAttachmentFromPayload(payload);
                attachment.Content = await retrieval(payload).ConfigureAwait(false);
                AddAttachment(attachment);
            }
        }

        private static Attachment CreateAttachmentFromPayload(Payload payload)
        {
            return new Attachment(payload.Id) { ContentType = payload.MimeType, Location = payload.Location };
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AS4Message other)
        {
            return GetPrimaryMessageId() == other.GetPrimaryMessageId();
        }

        #region Inner DetermineSizeStream class.

        private sealed class DetermineSizeStream : Stream
        {
            private long _length;

            /// <summary>When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
            /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
            /// <param name="count">The number of bytes to be written to the current stream. </param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                _length += count;
            }

            /// <summary>When overridden in a derived class, gets the length in bytes of the stream.</summary>
            /// <returns>A long value representing the length of the stream in bytes.</returns>
            public override long Length => _length;

            /// <summary>When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
            public override void Flush()
            {
                // Do Nothing
            }

            /// <summary>When overridden in a derived class, sets the position within the current stream.</summary>
            /// <returns>The new position within the current stream.</returns>
            /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
            /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
            public override long Seek(long offset, SeekOrigin origin)
            {
                return -1;
            }

            /// <summary>
            /// When overridden in a derived class, sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            /// <exception cref="InvalidOperationException"></exception>
            public override void SetLength(long value)
            {
                throw new InvalidOperationException();
            }

            /// <summary>When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
            /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
            /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
            /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
            /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports reading.</summary>
            /// <returns>true if the stream supports reading; otherwise, false.</returns>
            public override bool CanRead => false;

            /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports seeking.</summary>
            /// <returns>true if the stream supports seeking; otherwise, false.</returns>
            public override bool CanSeek => false;

            /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports writing.</summary>
            /// <returns>true if the stream supports writing; otherwise, false.</returns>
            public override bool CanWrite => true;

            /// <summary>When overridden in a derived class, gets or sets the position within the current stream.</summary>
            /// <returns>The current position within the stream.</returns>
            public override long Position { get; set; }
        }

        #endregion
    }
}