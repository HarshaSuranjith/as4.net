﻿using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation to transform
    /// incoming Payloads to a <see cref="InternalMessage"/>
    /// </summary>
    public class PayloadTransformer : ITransformer
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTransformer"/> class.
        /// </summary>
        public PayloadTransformer()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Tranform the Payload(s)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var internalMessage = new InternalMessage();

            Attachment attachment = CreateAttachmentFromReceivedMessage(message);
            internalMessage.AS4Message.AddAttachment(attachment);

            this._logger.Info("Transform the given Payload to a AS4 Attachment");
            return await Task.FromResult(internalMessage);
        }

        private static Attachment CreateAttachmentFromReceivedMessage(ReceivedMessage receivedMessage)
        {
            return new Attachment
            {
                Content = receivedMessage.RequestStream,
                ContentType = receivedMessage.ContentType
            };
        }
    }
}