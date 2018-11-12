﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
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
        public string EbmsRefToMessageId { get; set; }

        public string Exception { get; private set; }

        [MaxLength(512)]
        public string MessageLocation { get; private set; }

        public string PMode { get; private set; }

        [MaxLength(256)]
        public string PModeId { get; private set; }

        protected ExceptionEntity(
            string ebmsRefToMessageId,
            string messageLocation,
            Exception exception)
        {
            EbmsRefToMessageId = ebmsRefToMessageId;
            MessageLocation = messageLocation;
            Exception = exception.Message;
        }

        protected ExceptionEntity(
            string ebmsRefToMessageId,
            string messageLocation,
            string exception)
        {
            EbmsRefToMessageId = ebmsRefToMessageId;
            MessageLocation = messageLocation;
            Exception = exception;
        }

        /// <summary>
        /// Set the Id & string representation of the PMode that is used to process the message.
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
        /// Gets the sending processing mode of the child representation of an exception.
        /// </summary>
        public virtual SendingProcessingMode GetSendingPMode()
        {
            return null;
        }

        /// <summary>
        /// Gets the receiving processing mode of the child representation of an exception.
        /// </summary>
        /// <returns></returns>
        public virtual ReceivingProcessingMode GetReceivingPMode()
        {
            return null;
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

        [Column("Operation")]
        [MaxLength(50)]
        public Operation Operation { get; set; }
    }
}