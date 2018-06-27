﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Entity for Exceptions
    /// </summary>
    public class ExceptionEntity : Entity
    {
        [MaxLength(256)]
        public string EbmsRefToMessageId { get; private set; }

        public string Exception { get; private set; }

        public byte[] MessageBody { get; private set; }

        public string PMode { get; private set; }

        [MaxLength(256)]
        public string PModeId { get; private set; }

        /// <summary>
        /// Set the Id & string represenation of the PMode that is used to process the message.
        /// </summary>
        /// <param name="pmodeId"></param>
        /// <param name="pmodeContent"></param>
        public void SetPModeInformation(string pmodeId, string pmodeContent)
        {
            PModeId = pmodeId;
            PMode = pmodeContent;
        }

        /// <summary>
        /// Set the PMode that is used to process the message.
        /// </summary>
        /// <param name="pmode"></param>
        public async Task SetPModeInformationAsync(IPMode pmode)
        {
            if (pmode != null)
            {
                PModeId = pmode.Id;

                // The Xml Serializer is not able to serialize an interface, therefore
                // the argument must first be cast to a correct implementation.

                if (pmode is SendingProcessingMode sp)
                {
                    PMode = await AS4XmlSerializer.ToStringAsync(sp);
                }
                else if (pmode is ReceivingProcessingMode rp)
                {
                    PMode = await AS4XmlSerializer.ToStringAsync(rp);
                }
                else
                {
                    throw new NotImplementedException("Unable to serialize the the specified IPMode");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEntity"/> class.
        /// </summary>
        protected internal ExceptionEntity()
        {
            Operation = default(Operation);

            InsertionTime = DateTimeOffset.Now;
            ModificationTime = DateTimeOffset.Now;
        }

        protected ExceptionEntity(string ebmsRefToMessageId, Exception exception)
            : this(ebmsRefToMessageId, exception.ToString())
        {
        }

        protected ExceptionEntity(string ebmsRefToMessageId, string errorMessage) : this()
        {
            this.EbmsRefToMessageId = ebmsRefToMessageId;
            this.Exception = errorMessage;
        }

        protected ExceptionEntity(byte[] messageBody, Exception exception) : this(messageBody, exception.ToString())
        {
        }

        protected ExceptionEntity(byte[] messageBody, string errorMessage) : this()
        {
            this.MessageBody = messageBody;
            this.Exception = errorMessage;
        }

        [Obsolete]
        public void SetOperation(Operation operation)
        {
            Operation = operation;
        }

        [Column("Operation")]
        [MaxLength(50)]
        public Operation Operation { get; set; }

        /// <summary>
        /// Update the <see cref="Entity" /> to lock it with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity" /> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = value.ToEnum<Operation>();

            if (updatedOperation != Operation.NotApplicable)
            {
                Operation = updatedOperation;
            }
        }
    }
}