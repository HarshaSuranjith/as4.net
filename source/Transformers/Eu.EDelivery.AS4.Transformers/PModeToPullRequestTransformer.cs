﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation that's responsible for transformation PMode models to Pull Messages instances.
    /// </summary>
    public class PModeToPullRequestTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="receivedMessage">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public Task<MessagingContext> TransformAsync(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            if (receivedMessage.RequestStream == null)
            {
                return Task.FromResult(new MessagingContext(CreateAS4Exception("Invalid incoming request stream.")));
            }

            return TryCreatePullRequest(receivedMessage);
        }

        private static async Task<MessagingContext> TryCreatePullRequest(ReceivedMessage receivedMessage)
        {
            try
            {
                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive);

                receivedMessage.AssignPropertiesTo(context);
                
                SendingProcessingMode pmode = await DeserializeValidPMode(receivedMessage);
                context.SendingPMode = pmode;

                AS4Message as4Message = AS4Message.Create(new PullRequest(pmode.PullConfiguration.Mpc), pmode);

                return context.CloneWith(as4Message);
            }
            catch (AS4Exception exception)
            {
                return new MessagingContext(exception);
            }
            catch (Exception exception)
            {
                return new MessagingContext(CreateAS4Exception(exception.Message));
            }
        }

        private static async Task<SendingProcessingMode> DeserializeValidPMode(ReceivedMessage receivedMessage)
        {
            var pmode = await AS4XmlSerializer.FromStreamAsync<SendingProcessingMode>(receivedMessage.RequestStream);
            var validator = new SendingProcessingModeValidator();

            ValidationResult result = validator.Validate(pmode);

            if (result.IsValid)
            {
                return pmode;
            }

            throw ThrowHandleInvalidPModeException(pmode, result);
        }

        private static AS4Exception ThrowHandleInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                Logger.Error($"Sending PMode Validation Error: {error.PropertyName} = {error.ErrorMessage}");
            }

            string description = $"Sending PMode {pmode.Id} was invalid, see logging";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }

        private static AS4Exception CreateAS4Exception(string description)
        {
            Logger.Error(description);
            return AS4ExceptionBuilder.WithDescription(description).Build();
        }
    }
}