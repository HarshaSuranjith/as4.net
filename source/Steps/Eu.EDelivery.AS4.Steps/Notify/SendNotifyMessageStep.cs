﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how a <see cref="NotifyMessage"/> is sent to the business application 
    /// </summary>
    public class SendNotifyMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly INotifySenderProvider _provider;
        private readonly Func<DatastoreContext> _dataContextRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class
        /// </summary>
        public SendNotifyMessageStep()
            : this(Registry.Instance.NotifySenderProvider, Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep" /> class.
        /// Create a <see cref="IStep" /> implementation
        /// to send a <see cref="NotifyMessage" />
        /// to the consuming business application
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="dataContextRetriever">The data context retriever.</param>
        public SendNotifyMessageStep(INotifySenderProvider provider, Func<DatastoreContext> dataContextRetriever)
        {
            _provider = provider;
            _dataContextRetriever = dataContextRetriever;
        }

        /// <summary>
        /// Start sending <see cref="NotifyMessage"/>
        /// to the consuming business application
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            Logger.Info($"{messagingContext.Prefix} Start sending Notify Message...");

            if (messagingContext?.SendingPMode == null)
            {
                SendingProcessingMode pmode = RetrieveSendingPMode(messagingContext);
                if (pmode != null)
                {
                    messagingContext.SendingPMode = pmode;
                }
            }

            await TrySendNotifyMessage(messagingContext).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private SendingProcessingMode RetrieveSendingPMode(MessagingContext messagingContext)
        {
            using (DatastoreContext context = _dataContextRetriever())
            {
                var repo = new DatastoreRepository(context);
                return repo.RetrieveSendingPModeForOutMessage(messagingContext.NotifyMessage.MessageInfo.RefToMessageId);
            }
        }

        private async Task TrySendNotifyMessage(MessagingContext message)
        {
            try
            {
                NotifyMessageEnvelope notifyMessage = message.NotifyMessage;
                Method notifyMethod = GetNotifyMethod(message);
                await SendNotifyMessage(notifyMessage, notifyMethod).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw ThrowAS4SendException(exception, message);
            }
        }

        private static Method GetNotifyMethod(MessagingContext messagingContext)
        {
            NotifyMessageEnvelope notifyMessage = messagingContext.NotifyMessage;
            SendingProcessingMode sendPMode = messagingContext.SendingPMode;
            ReceivingProcessingMode receivePMode = messagingContext.ReceivingPMode;

            switch (notifyMessage.StatusCode)
            {
                case Status.Delivered: return sendPMode.ReceiptHandling.NotifyMethod;
                case Status.Error: return sendPMode.ErrorHandling.NotifyMethod;
                case Status.Exception: return DetermineMethod(sendPMode, sendPMode?.ExceptionHandling, receivePMode?.ExceptionHandling);
                default: throw new ArgumentOutOfRangeException($"Notify method not defined for status {notifyMessage.StatusCode}");
            }
        }

        private static Method DetermineMethod(IPMode sendPMode, SendHandling sendHandling, Receivehandling receivehandling)
        {
            return IsNotifyMessageFormedBySending(sendPMode) ? sendHandling?.NotifyMethod : receivehandling?.NotifyMethod;
        }

        private async Task SendNotifyMessage(NotifyMessageEnvelope notifyMessage, Method notifyMethod)
        {
            INotifySender sender = _provider.GetNotifySender(notifyMethod.Type);
            sender.Configure(notifyMethod);
            await sender.SendAsync(notifyMessage).ConfigureAwait(false);
        }

        private static AS4Exception ThrowAS4SendException(Exception innerException, MessagingContext message)
        {
            const string description = "Notify Message was not send correctly";
            Logger.Error(description);

            AS4ExceptionBuilder builder = AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(message.NotifyMessage.MessageInfo.MessageId)
                .WithErrorAlias(ErrorAlias.ConnectionFailure);

            AddPModeToBuilder(message, builder);

            return builder.Build();
        }

        private static void AddPModeToBuilder(MessagingContext message, AS4ExceptionBuilder builder)
        {
            if (IsNotifyMessageFormedBySending(message?.SendingPMode))
            {
                builder.WithSendingPMode(message?.SendingPMode);
            }
            else
            {
                builder.WithReceivingPMode(message?.ReceivingPMode);
            }
        }

        private static bool IsNotifyMessageFormedBySending(IPMode pmode)
        {
            return pmode?.Id != null;
        }
    }
}