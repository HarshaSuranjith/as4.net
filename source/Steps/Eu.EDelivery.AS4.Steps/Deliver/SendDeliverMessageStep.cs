﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how a DeliverMessage is sent to the consuming business application. 
    /// </summary>
    public class SendDeliverMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDeliverSenderProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// </summary>
        public SendDeliverMessageStep() : this(Registry.Instance.DeliverSenderProvider) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// for sending the Deliver Message to the consuming business application
        /// </summary>
        /// <param name="provider"> The provider. </param>
        public SendDeliverMessageStep(IDeliverSenderProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Start sending the AS4 Messages 
        /// to the consuming business application
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            Logger.Info($"[{messagingContext.DeliverMessage?.MessageInfo?.MessageId}] Start sending the Deliver Message to the consuming Business Application");

            await SendDeliverMessage(messagingContext).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task SendDeliverMessage(MessagingContext context)
        {
            if (context.ReceivingPMode == null)
            {
                throw new InvalidOperationException("Unable to send DeliverMessage: the MessagingContext does not contain a Receiving PMode");
            }

            if (context.ReceivingPMode.MessageHandling?.DeliverInformation == null)
            {
                throw new InvalidOperationException("Unable to send DeliverMessage: the ReceivingPMode does not contain any DeliverInformation");
            }

            DeliverMessageEnvelope deliverMessage = context.DeliverMessage;
            Method deliverMethod = context.ReceivingPMode.MessageHandling.DeliverInformation.DeliverMethod;

            IDeliverSender sender = _provider.GetDeliverSender(deliverMethod?.Type);
            sender.Configure(deliverMethod);
            await sender.SendAsync(deliverMessage).ConfigureAwait(false);
        }
    }
}