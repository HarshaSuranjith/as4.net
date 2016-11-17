﻿using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Signing;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a <see cref="AS4Message"/> signature gets verified
    /// </summary>
    public class VerifySignatureAS4MessageStep : IStep
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureAS4MessageStep"/> class
        /// </summary>
        public VerifySignatureAS4MessageStep()
        {
            this._certificateRepository = Registry.Instance.CertificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureAS4MessageStep"/> class
        /// Create a new Verify Signature AS4 Message Step,
        /// which will verify the Signature in the AS4 Message (if present)
        /// </summary>
        /// <param name="certificateRepository">The certificate Repository.</param>
        public VerifySignatureAS4MessageStep(ICertificateRepository certificateRepository)
        {
            this._certificateRepository = certificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start verifying the Signature of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when the signature cannot be verified</exception>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;

            PreConditions();
            if (MessageDoesNotNeedToBeVerified())
                return StepResult.SuccessAsync(internalMessage);
            StepResult stepResult = TryVerifyingSignature();

            return Task.FromResult(stepResult);
        }

        private void PreConditions()
        {
            AS4Message as4Message = this._internalMessage.AS4Message;
            ReceivingProcessingMode pmode = as4Message.ReceivingPMode;
            SigningVerification verification = pmode?.Security.SigningVerification;

            bool isMessageFailsTheRequiredSigning = verification?.Signature == Limit.Required && !as4Message.IsSigned;
            bool isMessageFailedTheUnallowedSigning = verification?.Signature == Limit.NotAllowed && as4Message.IsSigned;

            if (isMessageFailsTheRequiredSigning)
            {
                string description = $"Receiving PMode {pmode.Id} requires a Signed AS4 Message and the message is not";
                throw ThrowVerifySignatureAS4Exception(description, ErrorCode.Ebms0103);
            }

            if (!isMessageFailedTheUnallowedSigning) return;
            {
                string description = $"Receiving PMode {pmode.Id} doesn't allow a signed AS4 Message and the message is";
                throw ThrowVerifySignatureAS4Exception(description, ErrorCode.Ebms0103);
            }
        }

        private bool MessageDoesNotNeedToBeVerified()
        {
            return !this._internalMessage.AS4Message.IsSigned ||
                   this._internalMessage.AS4Message.ReceivingPMode?.Security
                       .SigningVerification.Signature == Limit.Ignored;
        }

        private StepResult TryVerifyingSignature()
        {
            try
            {
                return VerifySignature();
            }
            catch (Exception exception)
            {
                this._logger.Error(exception.Message);
                throw ThrowVerifySignatureAS4Exception("The Signature is invalid", ErrorCode.Ebms0101, exception);
            }
        }

        private StepResult VerifySignature()
        {
            if (!IsValidSignature())
                throw ThrowVerifySignatureAS4Exception("The Signature is invalid", ErrorCode.Ebms0101);

            this._logger.Info($"{this._internalMessage.Prefix} AS4 Message has a valid Signature present");
            return StepResult.Success(this._internalMessage);
        }

        private bool IsValidSignature()
        {
            AS4Message as4Message = this._internalMessage.AS4Message;
            VerifyConfig options = CreateVerifyOptionsForAS4Message();

            return as4Message.SecurityHeader.Verify(options);
        }

        private VerifyConfig CreateVerifyOptionsForAS4Message()
        {
            return new VerifyConfig
            {
                CertificateRepository = this._certificateRepository,
                Attachments = this._internalMessage.AS4Message.Attachments
            };
        }

        private AS4Exception ThrowVerifySignatureAS4Exception(
            string description, ErrorCode errorCode, Exception innerException = null)
        {
            description = this._internalMessage.Prefix + description;
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithMessageIds(this._internalMessage.AS4Message.MessageIds)
                .WithErrorCode(errorCode)
                .WithInnerException(innerException)
                .WithReceivingPMode(this._internalMessage.AS4Message.ReceivingPMode)
                .Build();
        }
    }
}