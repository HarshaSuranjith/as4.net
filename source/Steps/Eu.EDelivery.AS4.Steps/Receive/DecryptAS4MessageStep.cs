﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Strategies;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// The use case describes how a message gets decrypted.
    /// </summary>
    public class DecryptAS4MessageStep : IStep
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly ILogger _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptAS4MessageStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// to decrypt a <see cref="AS4Message"/>
        /// </summary>
        public DecryptAS4MessageStep()
        {
            this._certificateRepository = Registry.Instance.CertificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public DecryptAS4MessageStep(ICertificateRepository certificateRepository)
        {
            this._certificateRepository = certificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start Decrypting <see cref="AS4Message"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {            
            if (internalMessage.AS4Message.IsSignalMessage)
                return StepResult.SuccessAsync(internalMessage);

            PreConditions(internalMessage);

            if (IsEncryptedIgnored(internalMessage) || !internalMessage.AS4Message.IsEncrypted)
            {
                return StepResult.SuccessAsync(internalMessage);
            }

            TryDecryptAS4Message(internalMessage);
            return StepResult.SuccessAsync(internalMessage);
        }


        private void PreConditions(InternalMessage internalMessage)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            ReceivingProcessingMode pmode = as4Message.ReceivingPMode;
            Decryption decryption = pmode.Security.Decryption;

            if (decryption.Encryption == Limit.Required && !as4Message.IsEncrypted)
                throw ThrowCommonAS4Exception(
                    internalMessage,
                    $"AS4 Message is not encrypted but Receiving PMode {pmode.Id} requires it",
                    ErrorCode.Ebms0103);

            if (decryption.Encryption == Limit.NotAllowed && as4Message.IsEncrypted)
                throw ThrowCommonAS4Exception(
                    internalMessage,
                    $"AS4 Message is encrypted but Receiving PMode {pmode.Id} doesn't allow it",
                    ErrorCode.Ebms0103);
        }

        private bool IsEncryptedIgnored(InternalMessage internalMessage)
        {
            ReceivingProcessingMode pmode = internalMessage.AS4Message.ReceivingPMode;
            bool isIgnored = pmode.Security.Decryption.Encryption == Limit.Ignored;

            if (isIgnored)
            {
                this._logger.Info($"{internalMessage.Prefix} Decryption Receiving PMode {pmode.Id} is ignored");
            }

            return isIgnored;
        }

        private void TryDecryptAS4Message(InternalMessage internalMessage)
        {
            try
            {
                IEncryptionStrategy strategy = CreateEncryptStrategy(internalMessage);
                internalMessage.AS4Message.SecurityHeader.Decrypt(strategy);
                this._logger.Info($"{internalMessage.Prefix} AS4 Message is decrypted correctly");
            }
            catch (Exception exception)
            {
                throw ThrowCommonAS4Exception(internalMessage, "Decryption failed", ErrorCode.Ebms0102, exception);
            }
        }

        private IEncryptionStrategy CreateEncryptStrategy(InternalMessage internalMessage)
        {
            X509Certificate2 certificate = GetCertificate(internalMessage);
            AS4Message as4Message = internalMessage.AS4Message;

            EncryptionStrategyBuilder builder = new EncryptionStrategyBuilder(as4Message.EnvelopeDocument)
                .WithCertificate(certificate)
                .WithAttachments(as4Message.Attachments);

            return builder.Build();
        }

        private X509Certificate2 GetCertificate(InternalMessage internalMessage)
        {
            var findType = GetCertificateFindValue(internalMessage);

            return _certificateRepository.GetCertificate(findType.Item1, findType.Item2);
        }

        protected virtual Tuple<X509FindType, string> GetCertificateFindValue(InternalMessage internalMessage)
        {
            Decryption decryption = internalMessage.AS4Message.ReceivingPMode.Security.Decryption;
            return new Tuple<X509FindType, string>(decryption.PrivateKeyFindType, decryption.PrivateKeyFindValue);
        }
       
        private AS4Exception ThrowCommonAS4Exception(InternalMessage internalMessage,
            string description, ErrorCode errorCode, Exception exception = null)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            description = internalMessage.Prefix + description;
            this._logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)                
                .WithMessageIds(as4Message.MessageIds)
                .WithErrorCode(errorCode)
                .WithReceivingPMode(as4Message.ReceivingPMode)
                .Build();
        }
    }
}