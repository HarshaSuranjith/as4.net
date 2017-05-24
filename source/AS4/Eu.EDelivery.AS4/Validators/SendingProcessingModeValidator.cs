﻿using System;
using System.Linq;
using Castle.Core.Internal;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator responsible for Validation Model <see cref="SendingProcessingMode" />
    /// </summary>
    public class SendingProcessingModeValidator : AbstractValidator<SendingProcessingMode>,
                                                  IValidator<SendingProcessingMode>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public SendingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id).NotEmpty();

            RulesForPullConfiguration();
            RulesForPushConfiguration();
            RulesForReceiptHandling();
            RulesForErrorHandling();
            RulesForExceptionHandling();
            RulesForSigning();
            RulesForEncryption();
        }

        private void RulesForPullConfiguration()
        {
            Func<SendingProcessingMode, bool> isPulling =
                pmode => pmode.MepBinding == MessageExchangePatternBinding.Pull;

            RuleFor(pmode => pmode.PullConfiguration.Protocol).NotNull().When(isPulling);
            RuleFor(pmode => pmode.PullConfiguration.Protocol.Url).NotEmpty().When(isPulling);
        }

        private void RulesForPushConfiguration()
        {
            Func<SendingProcessingMode, bool> isPushing =
                pmode => pmode.MepBinding == MessageExchangePatternBinding.Push;

            RuleFor(pmode => pmode.PushConfiguration.Protocol).NotNull().When(isPushing);
            RuleFor(pmode => pmode.PushConfiguration.Protocol.Url).NotEmpty().When(isPushing);
        }

        private void RulesForReceiptHandling()
        {
            Func<SendingProcessingMode, bool> isReceiptHandlingEnabled =
                pmode => pmode.ReceiptHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod).NotNull().When(isReceiptHandlingEnabled);
            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(isReceiptHandlingEnabled);
            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod.Type).NotNull().When(isReceiptHandlingEnabled);
        }

        private void RulesForErrorHandling()
        {
            Func<SendingProcessingMode, bool> isErrorHandlingEnabled =
                pmode => pmode.ErrorHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod).NotNull().When(isErrorHandlingEnabled);
            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(isErrorHandlingEnabled);
            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod.Type).NotNull().When(isErrorHandlingEnabled);
        }

        private void RulesForExceptionHandling()
        {
            Func<SendingProcessingMode, bool> isExceptionHandlingEnabled =
                pmode => pmode.ExceptionHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod).NotNull().When(isExceptionHandlingEnabled);
            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(isExceptionHandlingEnabled);
            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Type).NotNull().When(isExceptionHandlingEnabled);
        }

        private void RulesForSigning()
        {
            Func<SendingProcessingMode, bool> isSigningEnabled = pmode => pmode.Security.Signing.IsEnabled;

            RuleFor(pmode => pmode.Security.Signing.PrivateKeyFindValue).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.Algorithm).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.HashFunction).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => Constants.Algoritms.Contains(pmode.Security.Signing.Algorithm))
                .NotNull()
                .When(isSigningEnabled);
            RuleFor(pmode => Constants.HashFunctions.Contains(pmode.Security.Signing.HashFunction))
                .NotNull()
                .When(isSigningEnabled);
        }

        private void RulesForEncryption()
        {
            Func<SendingProcessingMode, bool> isEncryptionEnabled = pmode => pmode.Security.Encryption.IsEnabled;
            RuleFor(pmode => pmode.Security.Encryption.PublicKeyFindValue).NotNull().When(isEncryptionEnabled);
        }

        /// <summary>
        /// Validate the given <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        void IValidator<SendingProcessingMode>.Validate(SendingProcessingMode model)
        {
            PreConditions(model);

            ValidationResult validationResult = base.Validate(model);

            if (!validationResult.IsValid)
            {
                throw ThrowHandleInvalidPModeException(model, validationResult);
            }
        }

        private static void PreConditions(SendingProcessingMode model)
        {
            try
            {
                ValidateKeySize(model);
            }
            catch (Exception exception)
            {
                Logger.Debug(exception);
            }
        }

        private static void ValidateKeySize(SendingProcessingMode model)
        {
            if (model.Security?.Encryption?.IsEnabled == false || model.Security?.Encryption == null)
            {
                return;
            }

            var keysizes = new[] {128, 192, 256};
            int actualKeySize = model.Security.Encryption.AlgorithmKeySize;

            if (!keysizes.Contains(actualKeySize) && model.Security?.Encryption != null)
            {
                const int defaultKeySize = 128;
                Logger.Warn($"Invalid Encryption 'Key Size': {actualKeySize}, {defaultKeySize} is taken as default");
                model.Security.Encryption.AlgorithmKeySize = defaultKeySize;
            }
        }

        private static AS4Exception ThrowHandleInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            foreach (ValidationFailure e in result.Errors)
            {
                Logger.Error($"Sending PMode Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }

            string description = $"Sending PMode {pmode.Id} was invalid, see logging";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }
    }
}