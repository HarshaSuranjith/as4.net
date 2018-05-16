﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how a DeliverMessage is sent to the consuming business application. 
    /// </summary>
    [Info("Send deliver message to the configured business application endpoint")]
    [Description("Send deliver message to the configured business application endpoint")]
    public class SendDeliverMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDeliverSenderProvider _messageProvider;
        private readonly Func<DatastoreContext> _createDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// </summary>
        public SendDeliverMessageStep()
            : this(
                Registry.Instance.DeliverSenderProvider,
                Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// for sending the Deliver Message to the consuming business application
        /// </summary>
        /// <param name="messageProvider"> The message sender provider.</param>
        /// <param name="createDbContext">Creates a new Db Context.</param>
        public SendDeliverMessageStep(
            IDeliverSenderProvider messageProvider,
            Func<DatastoreContext> createDbContext)
        {
            _messageProvider = messageProvider;
            _createDbContext = createDbContext;
        }

        /// <summary>
        /// Start sending the AS4 Messages 
        /// to the consuming business application
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException(
                    "Unable to send DeliverMessage: the MessagingContext does not contain a Receiving PMode");
            }

            if (messagingContext.ReceivingPMode.MessageHandling?.DeliverInformation == null)
            {
                throw new InvalidOperationException(
                    "Unable to send DeliverMessage: the ReceivingPMode does not contain any DeliverInformation");
            }

            Logger.Trace($"{messagingContext.LogTag} Start sending the deliver message to the consuming business application...");

            Method deliverMethod = messagingContext.ReceivingPMode.MessageHandling.DeliverInformation.DeliverMethod;

            IDeliverSender sender = _messageProvider.GetDeliverSender(deliverMethod?.Type);
            sender.Configure(deliverMethod);
            DeliverResult result = await sender.SendAsync(messagingContext.DeliverMessage).ConfigureAwait(false);

            await UpdateDeliverMessage(messagingContext, result);
            return StepResult.Success(messagingContext);
        }

        private async Task UpdateDeliverMessage(MessagingContext messagingContext, DeliverResult result)
        {
            using (DatastoreContext context = _createDbContext())
            {
                var repository = new DatastoreRepository(context);
                var retryService = new RetryService(repository);
                retryService.UpdateDeliverMessageAccordinglyToDeliverResult(
                    messagingContext.DeliverMessage.MessageInfo.MessageId,
                    result);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}