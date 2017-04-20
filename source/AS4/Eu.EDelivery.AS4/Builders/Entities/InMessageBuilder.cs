﻿using System;
using System.Threading;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="InMessage"/> Models
    /// </summary>
    public class InMessageBuilder
    {
        private readonly MessageUnit _messageUnit;
        private readonly AS4Message _as4Message;
        private string _pmodeString;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageBuilder"/> class. 
        /// Starting the Builder with a given Serialize Provider
        /// </summary>                
        private InMessageBuilder(MessageUnit messageUnit, AS4Message as4Message)
        {
            _messageUnit = messageUnit;
            _as4Message = as4Message;
        }

        /// <summary>
        /// Creates a new InMessageBuilder instance that can instantiate an <see cref="InMessage"/> for the received <paramref name="userMessage"/>
        /// </summary>
        /// <param name="userMessage"></param>
        /// <param name="belongsToAS4Message"></param>
        /// <returns></returns>
        public static InMessageBuilder ForUserMessage(UserMessage userMessage, AS4Message belongsToAS4Message)
        {
            return new InMessageBuilder(userMessage, belongsToAS4Message);
        }

        /// <summary>
        /// Creates a new InMessageBuilder instance that can instantiate an <see cref="InMessage"/> for the received <paramref name="signalMessage"/>
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <param name="belongsToAS4Message"></param>
        /// <returns></returns>
        public static InMessageBuilder ForSignalMessage(SignalMessage signalMessage, AS4Message belongsToAS4Message)
        {
            return new InMessageBuilder(signalMessage, belongsToAS4Message);
        }

        public InMessageBuilder WithPModeString(string pmode)
        {
            _pmodeString = pmode;
            return this;
        }

        /// <summary>
        /// Start Creating the <see cref="InMessage"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public InMessage Build(CancellationToken cancellationToken)
        {
            if (_as4Message == null)
            {
                throw new AS4Exception("Builder needs a AS4Message for building an InMessage");
            }
            if (_messageUnit == null)
            {
                throw new AS4Exception("Builder needs a Message Unit for building an InMessage");
            }

            return new InMessage
            {
                EbmsMessageId = _messageUnit.MessageId,
                EbmsRefToMessageId = _messageUnit.RefToMessageId,
                EbmsMessageType = DetermineMessageType(_messageUnit),
                ContentType = _as4Message.ContentType,
                Message = _as4Message,
                PMode = _pmodeString,
                MEP = MessageExchangePattern.Push, // TODO: this is hardcoded; is this correct ? Is this even relevant for inmsg ?
                Status = InStatus.Received,
                Operation = Operation.NotApplicable,
                InsertionTime = DateTimeOffset.UtcNow,
                ModificationTime = DateTimeOffset.UtcNow
            };
        }

        private static MessageType DetermineMessageType(MessageUnit messageUnit)
        {
            if (messageUnit is UserMessage)
            {
                return MessageType.UserMessage;
            }
            if (messageUnit is Receipt)
            {
                return MessageType.Receipt;
            }
            if (messageUnit is Error)
            {
                return MessageType.Error;
            }

            throw new InvalidOperationException("There is no MessageType mapped for this MessageUnit.");
        }

        ////private byte[] CreateMessageBody(AS4Message as4Message, CancellationToken token)
        ////{
        ////    var memoryStream = new MemoryStream();
        ////    ISerializer serializer = this._provider.Get(as4Message.ContentType);
        ////    serializer.Serialize(as4Message, memoryStream, token);

        ////    return memoryStream.ToArray();
        ////}
    }
}
