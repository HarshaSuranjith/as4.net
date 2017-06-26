﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Describes how the AS4 UserMessage is stored in the message store,
    /// in order to hand over to the Send Agents.
    /// </summary>
    public class StoreAS4MessageStep : IStep
    {
        // TODO: this class should be reviewed IMHO.  We should not save AS4Messages, but we should
        // save the MessagePart in the OutMessage table.  Each MessagePart has its own messagebody.
        // Right now, the MessageBody is the complete AS4Message; every OutMessage refers to that same messagebody which 
        // is not correct.
        // At this stage, there should be no AS4-message in my opinion, only UserMessages and SignalMessages.
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep" /> class.
        /// </summary>
        public StoreAS4MessageStep() : this(Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep"/> class.
        /// </summary>
        /// <param name="messageBodyStore">The as 4 Message Body Persister.</param>
        public StoreAS4MessageStep(IAS4MessageBodyStore messageBodyStore)
        {
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Store the <see cref="AS4Message" /> as OutMessage inside the DataStore
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken token)
        {
            await TryStoreOutMessagesAsync(messagingContext, token).ConfigureAwait(false);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task TryStoreOutMessagesAsync(MessagingContext message, CancellationToken token)
        {
            await StoreOutMessagesAsync(message, token).ConfigureAwait(false);
        }

        private async Task StoreOutMessagesAsync(MessagingContext message, CancellationToken token)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var service = new OutMessageService(new DatastoreRepository(context), _messageBodyStore);

                await service.InsertAS4Message(message, Operation.ToBeSent, token);

                await context.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}