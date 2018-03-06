﻿using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Services
{
    public interface IReceptionAwarenessService
    {
        /// <summary>
        /// Deadletters the OutMessage that has the specified <paramref name="outMessageId"/>
        /// </summary>
        /// <param name="outMessageId">The ID that uniquely identifies the OutMessage that must be deadlettered.</param>
        /// <param name="ebmsMessageId">The EBMS MessageId of the Message that must be deadlettered.</param>
        /// <param name="messageBodyStore">The message body persister.</param>
        /// <returns></returns>
        void DeadletterOutMessage(long outMessageId, string ebmsMessageId, IAS4MessageBodyStore messageBodyStore);

        /// <summary>
        /// Messages the needs to be resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        /// <returns></returns>
        bool MessageNeedsToBeResend(ReceptionAwareness awareness);

        /// <summary>
        /// Determines whether [is message already answered] [the specified awareness].
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        /// <returns>
        ///   <c>true</c> if [is message already answered] [the specified awareness]; otherwise, <c>false</c>.
        /// </returns>
        bool IsMessageAlreadyAnswered(ReceptionAwareness awareness);

        /// <summary>
        /// Updates for resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        void MarkReferencedMessageForResend(ReceptionAwareness awareness);

        /// <summary>
        /// Completes the message.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        void MarkReferencedMessageAsComplete(ReceptionAwareness awareness);

        /// <summary>
        /// Resets the referenced message.
        /// </summary>
        /// <param name="awarenes">The awarenes.</param>
        void ResetReferencedMessage(ReceptionAwareness awarenes);
    }
}