﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.ReceptionAwareness
{
    /// <summary>
    /// Describes how the AS4 message has to be behave in a Reception Awareness scenario
    /// </summary>
    public class ReceptionAwarenessUpdateDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        private Entities.ReceptionAwareness _receptionAwareness;        

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class
        /// </summary>
        public ReceptionAwarenessUpdateDatastoreStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            bool shouldWait = true;


            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                this._logger.Debug("Executing ReceptionAwarenessDataStoreStep");

                var repository = new DatastoreRepository(context);

                this._receptionAwareness = internalMessage.ReceptionAwareness;

                if (IsMessageAlreadyAnswered(repository))
                {
                    this._logger.Debug("Message has been answered, marking as complete");
                    UpdateForAnsweredMessage(repository);
                }
                else
                {
                    if (MessageNeedsToBeResend(repository))
                    {
                        this._logger.Debug(
                            $"Updating message for resending.  RetryCount = {_receptionAwareness.CurrentRetryCount}");
                        UpdateForResendMessage(repository);
                        shouldWait = false;
                    }
                    else
                    {
                        if (IsMessageUnanswered(repository))
                        {
                            this._logger.Debug("Message is unanswered.");
                            UpdateForUnansweredMessage(repository, cancellationToken);
                        }
                    }
                }
            }


            if (shouldWait)
            {
                WaitRetryInterval("Waiting retry interval...");
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        private bool IsMessageAlreadyAnswered(IDatastoreRepository repository)
        {
            string messageId = this._receptionAwareness.InternalMessageId;

            // TODO: Optimization:  do not retrieve the complete entity, but check count.            
            return repository.GetInMessage(
                inMessage => inMessage.EbmsRefToMessageId?.Equals(messageId) == true) != null;
        }

        private void UpdateForAnsweredMessage(IDatastoreRepository repository)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._logger.Info($"[{messageId}] Reception Awareness completed");
            UpdateReceptionAwareness(x => x.IsCompleted = true, repository);
        }

        private bool MessageNeedsToBeResend(IDatastoreRepository repository)
        {
            TimeSpan retryInterval = TimeSpan.Parse(this._receptionAwareness.RetryInterval);

            TimeSpan gracePeriod = TimeSpan.FromTicks(retryInterval.Ticks);

            DateTimeOffset deadlineForResend = this._receptionAwareness.LastSendTime.Add(gracePeriod);

            return
                this._receptionAwareness.CurrentRetryCount < this._receptionAwareness.TotalRetryCount &&
                repository.GetOutMessageById(this._receptionAwareness.InternalMessageId)?.Operation != Operation.Sending &&
                DateTimeOffset.UtcNow.CompareTo(deadlineForResend) > 0 &&
                this._receptionAwareness.IsCompleted == false;
        }

        private void UpdateForResendMessage(IDatastoreRepository repository)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._logger.Info($"[{messageId}] Update datastore so the ebMS message can be resend. (RetryCount = {this._receptionAwareness.CurrentRetryCount + 1})");

            repository.UpdateOutMessageAsync(messageId, x => x.Operation = Operation.ToBeSent);

        }

        private bool IsMessageUnanswered(IDatastoreRepository repository)
        {
            return this._receptionAwareness.CurrentRetryCount >= this._receptionAwareness.TotalRetryCount;
        }

        private async void UpdateForUnansweredMessage(IDatastoreRepository repository, CancellationToken cancellationToken)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._logger.Info($"[{messageId}] ebMS message is unanswered");

            UpdateReceptionAwareness(awareness => awareness.IsCompleted = true, repository);
            await repository.UpdateOutMessageAsync(messageId, x => x.Operation = Operation.DeadLettered);

            Error errorMessage = CreateError();
            AS4Message as4Message = CreateAS4Message(errorMessage, repository);

            await new InMessageService(repository).InsertErrorAsync(errorMessage, as4Message, cancellationToken);
        }

        private Error CreateError()
        {
            AS4Exception as4Exception = CreateAS4Exception();
            string messageId = _receptionAwareness.InternalMessageId;
            

            return new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithAS4Exception(as4Exception)
                .Build();
        }

        private AS4Exception CreateAS4Exception()
        {
            string messageId = this._receptionAwareness.InternalMessageId;

            return AS4ExceptionBuilder
                .WithDescription($"[{messageId}] Missing Receipt")
                .WithMessageIds(this._receptionAwareness.InternalMessageId)
                .WithErrorCode(ErrorCode.Ebms0301)
                .Build();
        }

        private AS4Message CreateAS4Message(SignalMessage errorMessage, IDatastoreRepository repository)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            OutMessage outMessage = repository.GetOutMessageById(messageId);
            var pmode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);

            var builder = new AS4MessageBuilder()
                .WithSendingPMode(pmode)
                .WithSignalMessage(errorMessage);

            return builder.Build();
        }

        private void UpdateReceptionAwareness(Action<Entities.ReceptionAwareness> updateAction, IDatastoreRepository repository)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            repository.UpdateReceptionAwarenessAsync(messageId, updateAction);
        }

        private void WaitRetryInterval(string description)
        {
            TimeSpan retryInterval = TimeSpan.Parse(this._receptionAwareness.RetryInterval);
            string messageId = this._receptionAwareness.InternalMessageId;

            this._logger.Info($"[{messageId}] {description}");

            Thread.Sleep(retryInterval);
        }
    }
}