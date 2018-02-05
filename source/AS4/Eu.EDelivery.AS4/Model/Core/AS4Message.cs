﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using MimeKit;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Internal AS4 Message between MSH
    /// </summary>
    public class AS4Message : IEquatable<AS4Message>
    {
        private readonly bool _serializeAsMultiHop;

        /// <summary>
        /// Prevents a default instance of the <see cref="AS4Message"/> class from being created.
        /// </summary>
        /// <param name="serializeAsMultiHop">if set to <c>true</c> [serialize as multi hop].</param>
        private AS4Message(bool serializeAsMultiHop = false)
        {
            _serializeAsMultiHop = serializeAsMultiHop;
            ContentType = "application/soap+xml";
            SigningId = new SigningId();
            SecurityHeader = new SecurityHeader();
            Attachments = new List<Attachment>();
            _messageUnits = new List<MessageUnit>();
        }

        public static AS4Message Empty => new AS4Message(serializeAsMultiHop: false);

        public string ContentType { get; set; }

        public XmlDocument EnvelopeDocument { get; private set; }

        // ReSharper disable once InconsistentNaming
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

        private readonly List<MessageUnit> _messageUnits;

        public IEnumerable<MessageUnit> MessageUnits => _messageUnits.AsReadOnly();

        public IEnumerable<UserMessage> UserMessages => MessageUnits.OfType<UserMessage>();

        public IEnumerable<SignalMessage> SignalMessages => MessageUnits.OfType<SignalMessage>();

        public ICollection<Attachment> Attachments { get; private set; }

        public SigningId SigningId { get; set; }

        public SecurityHeader SecurityHeader { get; set; }

        public string[] MessageIds
            => UserMessages.Select(m => m.MessageId).Concat(SignalMessages.Select(m => m.MessageId)).ToArray();

        public UserMessage PrimaryUserMessage => UserMessages.FirstOrDefault();

        public SignalMessage PrimarySignalMessage => SignalMessages.FirstOrDefault();

        public bool IsSignalMessage => MessageUnits.FirstOrDefault() is SignalMessage;

        public bool IsUserMessage => MessageUnits.FirstOrDefault() is UserMessage;

        public bool IsSigned => SecurityHeader.IsSigned;

        public bool IsEncrypted => SecurityHeader.IsEncrypted;

        public bool HasAttachments => Attachments?.Any() ?? false;

        public bool IsEmpty => PrimarySignalMessage == null && PrimaryUserMessage == null;

        public bool IsPullRequest => PrimarySignalMessage is PullRequest;

        /// <summary>
        /// Creates message with a SOAP envelope.
        /// </summary>
        /// <param name="soapEnvelope">The SOAP envelope.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="securityHeader"></param>
        /// <param name="messagingHeader"></param>
        /// <param name="bodyElement"></param>
        ///<remarks>This method should only be used when creating an AS4 Message via deserialization.</remarks>
        /// <returns></returns>
        internal static AS4Message Create(XmlDocument soapEnvelope, string contentType, SecurityHeader securityHeader, Xml.Messaging messagingHeader, Xml.Body1 bodyElement)
        {
            if (soapEnvelope == null)
            {
                throw new ArgumentNullException(nameof(soapEnvelope));
            }

            if (String.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException(@"ContentType must be defined.", nameof(contentType));
            }

            if (securityHeader == null)
            {
                throw new ArgumentNullException(nameof(securityHeader));
            }

            if (messagingHeader == null)
            {
                throw new ArgumentNullException(nameof(messagingHeader));
            }

            var result = new AS4Message
            {
                EnvelopeDocument = soapEnvelope,
                ContentType = contentType,
                SecurityHeader = securityHeader
            };

            result.SigningId.HeaderSecurityId = messagingHeader.SecurityId;

            if (bodyElement?.AnyAttr != null)
            {
                result.SigningId.BodySecurityId = bodyElement.AnyAttr.FirstOrDefault(a => a.LocalName == "Id")?.Value;
            }

            var units = SoapEnvelopeSerializer.GetMessageUnitsFromMessagingHeader(messagingHeader);

            result._messageUnits.AddRange(units);

            return result;
        }

        /// <summary>
        /// Creates message with a <see cref="SendingProcessingMode"/>.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4Message Create(SendingProcessingMode pmode)
        {
            return new AS4Message(pmode?.MessagePackaging?.IsMultiHop == true);
        }

        public static AS4Message Create(MessageUnit message, SendingProcessingMode pmode = null)
        {
            AS4Message as4Message = Create(pmode);

            as4Message.AddMessageUnit(message);

            return as4Message;
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
        /// Adds a <see cref="MessageUnit"/> to the AS4 Message.
        /// </summary>
        /// <param name="messageUnit">The MessageUnit, which can be a signalmessage or a usermessage.</param>
        /// <remarks>Adding a MessageUnit will cause the EnvelopeDocument property to be set to null, since the 
        /// Envelope Document will no longer be in-sync.</remarks>
        public void AddMessageUnit(MessageUnit messageUnit)
        {
            _messageUnits.Add(messageUnit);
            EnvelopeDocument = null;
        }

        /// <summary>
        /// Removes the <paramref name="messageUnit"/> from the AS4 Message.
        /// </summary>
        /// <param name="messageUnit"></param>
        /// <remarks>Removing a MessageUnit will cause the EnvelopeDocument property to be set to null, since the 
        /// Envelope Document will no longer be in-sync.</remarks>
        public void RemoveMessageUnit(MessageUnit messageUnit)
        {
            _messageUnits.Remove(messageUnit);
            EnvelopeDocument = null;
        }

        /// <summary>
        /// Clears the MessageUnit collection.
        /// </summary>
        /// <remarks>Clearing the essageUnits will cause the EnvelopeDocument property to be set to null, since the 
        /// Envelope Document will no longer be in-sync.</remarks>
        public void ClearMessageUnits()
        {
            _messageUnits.Clear();
            EnvelopeDocument = null;
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

        /// <summary>
        /// Compresses the Attachments that are part of this AS4 Message and
        /// modifies the Payload-info in the UserMessage to indicate that the attachment 
        /// is compressed.
        /// </summary>
        public void CompressAttachments()
        {
            foreach (Attachment attachment in this.Attachments)
            {
                CompressAttachment(attachment);
                AssignAttachmentProperties(attachment);
            }
            // Since the headers in the message have changed, the EnvelopeDocument
            // is no longer in sync and should be set to null.
            this.EnvelopeDocument = null;
        }

        private static void CompressAttachment(Attachment attachment)
        {
            VirtualStream outputStream =
                VirtualStream.CreateVirtualStream(
                    attachment.EstimatedContentSize > -1 ? attachment.EstimatedContentSize : VirtualStream.ThresholdMax);

            var compressionLevel = DetermineCompressionLevelFor(attachment);

            using (var gzipCompression = new GZipStream(outputStream, compressionLevel, leaveOpen: true))
            {
                attachment.Content.CopyTo(gzipCompression);
            }

            outputStream.Position = 0;
            attachment.Content = outputStream;
        }

        private static CompressionLevel DetermineCompressionLevelFor(Attachment attachment)
        {
            if (attachment.ContentType.Equals("application/gzip", StringComparison.OrdinalIgnoreCase))
            {
                // In certain cases, we do not want to waste time compressing the attachment, since
                // compressing will only take time without noteably decreasing the attachment size.
                return CompressionLevel.NoCompression;
            }

            if (attachment.EstimatedContentSize > -1)
            {
                const long twelveKilobytes = 12_288;
                const long twoHundredMegabytes = 209_715_200;

                if (attachment.EstimatedContentSize <= twelveKilobytes)
                {
                    return CompressionLevel.NoCompression;
                }

                if (attachment.EstimatedContentSize > twoHundredMegabytes)
                {
                    return CompressionLevel.Fastest;
                }
            }

            return CompressionLevel.Optimal;
        }

        private static void AssignAttachmentProperties(Attachment attachment)
        {
            attachment.Properties["CompressionType"] = "application/gzip";
            attachment.Properties["MimeType"] = attachment.ContentType;
            attachment.ContentType = "application/gzip";
        }

        /// <summary>
        /// Decrypt the AS4 Message using the specified <paramref name="certificate"/>.
        /// </summary>
        /// <param name="certificate"></param>
        public void Decrypt(X509Certificate2 certificate)
        {
            var decryptor = DecryptionStrategyBuilder.Create(this)
                                                   .WithCertificate(certificate)
                                                   .Build();
            
            decryptor.DecryptMessage();
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AS4Message other)
        {
            if (other == null)
            {
                return false;
            }

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