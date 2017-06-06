﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a AS4 Receipt gets stores in the Data store
    /// </summary>
    public class StoreAS4ReceiptStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReceiptStep"/> class.
        /// </summary>
        public StoreAS4ReceiptStep() : this(Registry.Instance.MessageBodyStore) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReceiptStep" /> class
        /// </summary>
        /// <param name="messageBodyStore">The message body persister.</param>
        public StoreAS4ReceiptStep(IAS4MessageBodyStore messageBodyStore)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Start storing the AS4 Receipt
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message.IsEmpty)
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                await new OutMessageService(repository, _messageBodyStore).InsertAS4Message(messagingContext, Operation.NotApplicable, cancellationToken);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.Info($"{messagingContext.Prefix} Store AS4 Receipt into the Datastore");
            }

            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}
