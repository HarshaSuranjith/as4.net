﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// AS4 Message Entity
    /// </summary>
    public abstract class MessageEntity : Entity
    {
        public string EbmsMessageId { get; set; }

        public string EbmsRefToMessageId { get; set; }

        [MaxLength(256)]
        public string ContentType { get; set; }

        public string PMode { get; set; }

        /// <summary>
        /// Gets or sets the AS4Message instance for which this MessageEntity is an instance.
        /// </summary>
        /// <remarks>
        /// This property is not persisted to the Datastore.  It is used to persist the Message in another location by an
        /// <see cref="IAS4MessageBodyStore" />
        /// </remarks>
        [NotMapped]
        internal AS4Message Message { get; set; }

        public string FromParty { get; set; }

        public string ToParty { get; set; }

        [Column("MPC")]
        public string Mpc { get; set; }

        public string ConversationId { get; set; }

        public string Service { get; set; }

        public string Action { get; set; }

        public bool IsDuplicate { get; set; }

        public bool IsTest { get; set; }

        /// <summary>
        /// Gets to the location where the AS4Message body can be found.
        /// </summary>
        [MaxLength(512)]
        public string MessageLocation { get; internal set; }

        [Obsolete("The Message Body is no longer stored in the Datastore")]
        public byte[] MessageBody { get; set; }

        [NotMapped]
        public Operation Operation { get; set; }

        [Column("Operation")]
        [MaxLength(50)]
        public string OperationString
        {
            get { return Operation.ToString(); }
            set { Operation = (Operation) Enum.Parse(typeof(Operation), value, true); }
        }

        public DateTimeOffset InsertionTime { get; set; }

        public DateTimeOffset ModificationTime { get; set; }

        [NotMapped]
        public MessageExchangePattern MEP { get; set; }

        [NotMapped]
        public MessageType EbmsMessageType { get; set; }

        [NotMapped]
        public ErrorAlias ErrorAlias { get; set; }

        [Column("MEP")]
        [MaxLength(25)]
        public string MEPString
        {
            get { return MEP.ToString(); }
            set { MEP = (MessageExchangePattern) Enum.Parse(typeof(MessageExchangePattern), value, true); }
        }

        [Column("EbmsMessageType")]
        [MaxLength(50)]
        public string EbmsMessageTypeString
        {
            get { return EbmsMessageType.ToString(); }
            set { EbmsMessageType = (MessageType) Enum.Parse(typeof(MessageType), value, true); }
        }

        [Column("ExceptionType")]
        [MaxLength(75)]
        public string ExceptionTypeString
        {
            get { return ErrorAlias.ToString(); }
            set { ErrorAlias = (ErrorAlias) Enum.Parse(typeof(ErrorAlias), value, true); }
        }

        [Column("Status")]
        [MaxLength(50)]
        public abstract string StatusString { get; set; }

        public string SoapEnvelope { get; set; }

        /// <summary>
        /// Update the <see cref="Entity" /> to lock it with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity" /> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = (Operation) Enum.Parse(typeof(Operation), value, true);

            if (updatedOperation != Operation.NotApplicable)
            {
                Operation = updatedOperation;
            }
        }

        /// <summary>
        /// Retrieves the Message body as a stream.
        /// </summary>
        /// <param name="store">
        /// The <see cref="MessageBodyStore" /> which is responsible for providing the correct
        /// <see cref="IAS4MessageBodyStore" /> that loads the <see cref="AS4Message" /> body.
        /// </param>
        /// <returns>A Stream which contains the MessageBody</returns>
        public async Task<Stream> RetrieveMessagesBody(IAS4MessageBodyStore store)
        {
            if (string.IsNullOrWhiteSpace(MessageLocation))
            {
                LogManager.GetCurrentClassLogger().Warn("Unable to retrieve the AS4 Message Body: MessageLocation is not set.");
                return null;
            }

            return await TryLoadMessageBody(store);
        }

        private async Task<Stream> TryLoadMessageBody(IAS4MessageBodyStore store)
        {
            try
            {
                return await store.LoadMessagesBody(MessageLocation);
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Error(exception.Message);

                return null;
            }
        }
    }
}