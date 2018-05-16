﻿using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service abstraction to set the referenced deliver message to the right Status/Operation accordingly to the <see cref="DeliverResult"/>.
    /// </summary>
    public class RetryService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatastoreRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryService"/> class.
        /// </summary>
        /// <param name="respository">The repository.</param>
        public RetryService(IDatastoreRepository respository)
        {
            _repository = respository;
        }

        /// <summary>
        /// Updates the message Status/Operation accordingly to <see cref="DeliverResult"/>.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public void UpdateDeliverMessageAccordinglyToDeliverResult(string messageId, DeliverResult result)
        {
            _repository.UpdateInMessage(
                messageId,
                inMessage =>
                {
                    if (result.Status == DeliveryStatus.Successful)
                    {
                        Logger.Info($"(Deliver)[{messageId}] Mark deliver message as Delivered");
                        Logger.Debug($"(Deliver)[{messageId}] Update InMessage with Status and Operation set to Delivered");

                        inMessage.SetStatus(InStatus.Delivered);
                        inMessage.SetOperation(Operation.Delivered);
                    }
                    else
                    {
                        (int current, int max) = _repository
                            .GetInMessageData(messageId, m => Tuple.Create(m.CurrentRetryCount, m.MaxRetryCount));

                        if (current < max && result.EligeableForRetry)
                        {
                            Logger.Info($"(Deliver)[{messageId}] DeliverMessage failed this time, will be retried");
                            Logger.Debug($"(Deliver[{messageId}]) Update InMessage with CurrentRetryCount={current + 1}, Operation=ToBeDelivered");

                            inMessage.CurrentRetryCount = current + 1;
                            inMessage.SetOperation(Operation.ToBeDelivered);
                        }
                        else
                        {
                            Logger.Info($"(Deliver)[{messageId}] DeliverMessage failed during the delivery, exhausted retries");
                            Logger.Debug($"(Deliver)[{messageId}] Update InMessage with Status=Exception, Operation=DeadLettered");

                            inMessage.SetStatus(InStatus.Exception);
                            inMessage.SetOperation(Operation.DeadLettered);
                        }
                    }
                });
        }
    }
}
