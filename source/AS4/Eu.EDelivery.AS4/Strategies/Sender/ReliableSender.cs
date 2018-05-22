﻿using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Decorator to add the 'reliable' functionality of the sending functionality 
    /// to both the <see cref="IDeliverSender"/> and <see cref="INotifySender"/> implementation.
    /// </summary>
    internal class ReliableSender : IDeliverSender, INotifySender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDeliverSender _deliverSender;
        private readonly INotifySender _notifySender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableSender"/> class.
        /// </summary>
        /// <param name="deliverSender"></param>
        public ReliableSender(IDeliverSender deliverSender)
        {
            _deliverSender = deliverSender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableSender"/> class.
        /// </summary>
        /// <param name="notifySender"></param>
        public ReliableSender(INotifySender notifySender)
        {
            _notifySender = notifySender;
        }

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _deliverSender?.Configure(method);
            _notifySender?.Configure(method);
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public async Task<SendResult> SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            return await SendMessageResult(
                    message: deliverMessage,
                    sending: _deliverSender.SendAsync,
                    exMessage: $"(Deliver)[{deliverMessage?.MessageInfo?.MessageId}] Unable to send DeliverMessage to the configured endpoint due to an exception")
                .ConfigureAwait(false);
        }

        private static async Task<SendResult> SendMessageResult<T>(
            T message,  
            Func<T, Task<SendResult>> sending, 
            string exMessage)
        {
            try
            {
                return await sending(message);
            }
            catch (Exception ex)
            {
                LogExceptionIncludingInner(ex, exMessage);
                return SendResult.FatalFail;
            }
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public async Task<SendResult> SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            return await SendMessageResult(
                message: notifyMessage,
                sending: notifyMessage1 => _notifySender.SendAsync(notifyMessage1),
                exMessage: $"(Notify)[{notifyMessage?.MessageInfo?.MessageId}] Unable to send NotifyMessage to the configured endpoint due to and exceptoin")
                .ConfigureAwait(false);
        }

        private static void LogExceptionIncludingInner(Exception ex, string exMessage)
        {

            Logger.Error(exMessage);

            if (ex.InnerException != null)
            {
                Logger.Error(ex.InnerException.Message);
            }
        }
    }
}
