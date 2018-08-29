﻿using System;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Incoming Message Exception Data Entity Schema
    /// </summary>
    public class InException : ExceptionEntity
    {
        // ReSharper disable once UnusedMember.Local - default ctor is required for Entity Framework.
        private InException() { }

        private InException(
            string ebmsRefToMessageId, 
            string exceptionLocation,
            Exception exception) : base(ebmsRefToMessageId, exceptionLocation, exception) { }

        // TODO: is used in tests and should be looked at if we really need this ctor.
        internal InException(
            string ebmsRefToMessageId,
            string exceptionLocation,
            string exception) : base(ebmsRefToMessageId, exceptionLocation, exception) { }


        /// <summary>
        /// Sets the <see cref="InException.Operation"/> based on the configuration in the specified <paramref name="exceptionHandling"/>.
        /// </summary>
        /// <param name="exceptionHandling">The exception handling of the <see cref="ReceivingProcessingMode"/></param>
        public InException SetOperationFor(ReceiveHandling exceptionHandling)
        {
            bool needsToBeNotified = exceptionHandling?.NotifyMessageConsumer == true;
            Operation = needsToBeNotified ? Operation.ToBeNotified : default(Operation);
            return this;
        }

        /// <summary>
        /// Creates an <see cref="OutException"/> that references an exsiting stored message.
        /// </summary>
        /// <param name="ebmsRefToMessageId">The message id of the message that caused the exception.</param>
        /// <param name="exception">The occurred exception for which we have to insert a record.</param>
        public static InException ForEbmsMessageId(string ebmsRefToMessageId, Exception exception)
        {
            return new InException(ebmsRefToMessageId: ebmsRefToMessageId, exceptionLocation: null, exception: exception);
        }

        /// <summary>
        /// Creates an <see cref="InException"/> that uses a stored location of the original message because it can't be referenced to an existing stored message.
        /// </summary>
        /// <param name="messageLocation">The location to where the refering message which caused the exception is stored.</param>
        /// <param name="exception">The occurred exception for which we have to insert a record.</param>
        public static InException ForMessageBody(string messageLocation, Exception exception)
        {
            return new InException(ebmsRefToMessageId: null, exceptionLocation: messageLocation, exception: exception);
        }
    }
}