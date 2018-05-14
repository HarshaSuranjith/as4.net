﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is delivered
    /// </summary>
    [Description("This step makes sure that the status of the message is correctly set after the message has been delivered.")]
    [Info("Update message status after delivery")]
    [Obsolete("The functionality of this step is now embeded in the " + nameof(SendDeliverMessageStep) + " making this step obsolete")]
    public class DeliverUpdateDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start updating the InMessages
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            Logger.Info($"{messagingContext.EbmsMessageId} Update AS4 UserMessages in Datastore");

            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                string messageId = messagingContext.DeliverMessage.MessageInfo.MessageId;
                Logger.Info($"[{messageId}] Update InMessage with Delivered Status and Operation");

                repository.UpdateInMessage(messageId, inMessage =>
                {
                    inMessage.SetStatus(InStatus.Delivered);
                    inMessage.SetOperation(Operation.Delivered);
                });

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}
