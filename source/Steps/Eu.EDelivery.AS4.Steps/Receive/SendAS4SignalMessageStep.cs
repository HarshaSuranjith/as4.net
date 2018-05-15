﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    [Info("Send AS4 signal message")]
    [Description("Send AS4 signal message back to the original sender")]
    public class SendAS4SignalMessageStep : IStep
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4SignalMessageStep" /> class.
        /// </summary>
        public SendAS4SignalMessageStep()
            : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4SignalMessageStep" /> class.
        /// </summary>
        public SendAS4SignalMessageStep(
            Func<DatastoreContext> createDatastoreContext,
            IAS4MessageBodyStore messageBodyStore)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Start executing the Receipt Decorator
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                return StepResult.Success(messagingContext);
            }

            using (DatastoreContext dataContext = _createDatastoreContext())
            {
                var outService = new OutMessageService(
                    new DatastoreRepository(dataContext),
                    _messageBodyStore);

                outService.InsertAS4Message(messagingContext, Operation.NotApplicable);

                await dataContext.SaveChangesAsync().ConfigureAwait(false);
            }

            if (IsReplyPatternCallback(messagingContext))
            {
                return CreateEmptySoapResult(messagingContext);
            }

            if (Logger.IsInfoEnabled)
            {
                string signalMessageType =
                    messagingContext.AS4Message.PrimarySignalMessage is Receipt
                        ? "Receipt"
                        : "Error";

                Logger.Info(
                    $"{messagingContext.LogTag} {signalMessageType} will be written to the response");
            }

            return StepResult.Success(messagingContext);
        }

        private static bool IsReplyPatternCallback(MessagingContext message)
        {
            return message.ReceivingPMode?.ReplyHandling?.ReplyPattern == ReplyPattern.Callback;
        }

        private static StepResult CreateEmptySoapResult(MessagingContext messagingContext)
        {
            Logger.Info(
                $"{messagingContext} Empty Accepted response will be send " + 
                "to requested party since signal will be sent async");

            return StepResult.Success(
                new MessagingContext(
                    AS4Message.Create(messagingContext.SendingPMode),
                    MessagingContextMode.Receive)
                {
                    ReceivingPMode = messagingContext.ReceivingPMode
                });
        }
    }
}
