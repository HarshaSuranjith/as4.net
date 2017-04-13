﻿using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the Messages.Out and Messages.In data stores get updated
    /// </summary>
    public class SendUpdateDataStoreStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep" /> class
        /// </summary>
        public SendUpdateDataStoreStep() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        public SendUpdateDataStoreStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Execute the Update DataStore Step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var inMessageService = new InMessageService(new DatastoreRepository(context));
                var signalMessageUpdate = new SendSignalMessageUpdate(internalMessage, inMessageService, cancellationToken);

                signalMessageUpdate.StartUpdatingSignalMessages();
                await context.SaveChangesAsync(cancellationToken);
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        /// <summary>
        /// Method Object for the <see cref="SignalMessage"/> instances.
        /// </summary>
        private class SendSignalMessageUpdate
        {
            private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
            private readonly InternalMessage _originalMessage;
            private readonly IInMessageService _messageService;
            private readonly CancellationToken _cancellation;

            /// <summary>
            /// Initializes a new instance of the <see cref="SendSignalMessageUpdate"/> class.
            /// </summary>
            public SendSignalMessageUpdate(
                InternalMessage originalMessage,
                IInMessageService messageService,
                CancellationToken cancellation)
            {
                _originalMessage = originalMessage;
                _messageService = messageService;
                _cancellation = cancellation;
            }

            /// <summary>
            /// Start updating the <see cref="SignalMessage"/> instances for the 'Send' operation.
            /// </summary>
            public void StartUpdatingSignalMessages()
            {
                foreach (SignalMessage signalMessage in _originalMessage.AS4Message.SignalMessages)
                {
                    Logger.Info($"{_originalMessage.Prefix} Update SignalMessage {signalMessage.MessageId}");

                    TryUpdateSignalMessage(signalMessage);
                }
            }

            private void TryUpdateSignalMessage(SignalMessage signalMessage)
            {
                try
                {
                    UpdateSignalMessage(signalMessage);
                }
                catch (Exception exception)
                {
                    string description = $"Unable to update SignalMessage {signalMessage.MessageId}";
                    throw ThrowAS4UpdateDatastoreException(description, exception);
                }
            }

            private void UpdateSignalMessage(SignalMessage signalMessage)
            {
                if (signalMessage is Receipt)
                {
                    UpdateReceipt(signalMessage);
                }
                else if (signalMessage is Error)
                {
                    UpdateError(signalMessage);
                }
                else
                {
                    UpdateOther(signalMessage);
                }
            }

            private void UpdateReceipt(SignalMessage signalMessage)
            {
                if (signalMessage is Receipt receipt && receipt.NonRepudiationInformation == null)
                {
                    receipt.NonRepudiationInformation = CreateNonRepudiationInformation();
                }

                _messageService.InsertReceipt(signalMessage, _originalMessage.AS4Message, _cancellation);

                _messageService.UpdateSignalMessage(signalMessage, OutStatus.Ack, _cancellation);
            }

            private NonRepudiationInformation CreateNonRepudiationInformation()
            {
                ArrayList references = _originalMessage.AS4Message.SecurityHeader.GetReferences();

                return new NonRepudiationInformationBuilder().WithSignedReferences(references).Build();
            }

            private void UpdateError(SignalMessage signalMessage)
            {
                _messageService.InsertError(signalMessage, _originalMessage.AS4Message, _cancellation);

                _messageService.UpdateSignalMessage(signalMessage, OutStatus.Nack, _cancellation);
            }

            private void UpdateOther(SignalMessage signalMessage)
            {
                _messageService.UpdateSignalMessage(signalMessage, OutStatus.Sent, _cancellation);
            }

            private AS4Exception ThrowAS4UpdateDatastoreException(string description, Exception innerException)
            {
                Logger.Error(description);

                return AS4ExceptionBuilder
                    .WithDescription(description)
                    .WithInnerException(innerException)
                    .WithMessageIds(_originalMessage.AS4Message.MessageIds)
                    .WithSendingPMode(_originalMessage.AS4Message.SendingPMode)
                    .Build();
            }
        }
    }
}