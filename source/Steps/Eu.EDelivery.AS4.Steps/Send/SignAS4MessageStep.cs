﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Strategies;
using NLog;
using System.Security.Cryptography;
using Eu.EDelivery.AS4.Security.Signing;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the MSH signs the AS4 UserMessage
    /// </summary>
    [Info("Sign the AS4 Message")]
    [Description("This step signs the AS4 Message if signing is enabled in the Sending PMode.")]
    public class SignAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICertificateRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class
        /// </summary>
        public SignAS4MessageStep()
        {
            _repository = Registry.Instance.CertificateRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class. 
        /// Create Signing Step with a given Certificate Store Repository
        /// </summary>
        /// <param name="repository">
        /// </param>
        public SignAS4MessageStep(ICertificateRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Sign the <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            if (messagingContext.SendingPMode?.Security.Signing.IsEnabled != true)
            {
                Logger.Info($"{messagingContext.EbmsMessageId} Sending PMode {messagingContext.SendingPMode?.Id} Signing is disabled");
                return await StepResult.SuccessAsync(messagingContext);
            }

            TrySignAS4Message(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void TrySignAS4Message(MessagingContext message)
        {
            try
            {
                Logger.Info($"{message.EbmsMessageId} Sign AS4 Message with given Signing Information");
                SignAS4Message(message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }
                Logger.Trace(exception.StackTrace);
                throw;
            }
        }

        private void SignAS4Message(MessagingContext message)
        {
            X509Certificate2 certificate = RetrieveCertificate(message);

            if (!certificate.HasPrivateKey)
            {
                throw new CryptographicException($"{message.EbmsMessageId} Certificate does not have a private key");
            }

            ICalculateSignatureStrategy signingStrategy = CreateSignStrategy(message, certificate);
            message.AS4Message.SecurityHeader.Sign(signingStrategy);
        }

        private X509Certificate2 RetrieveCertificate(MessagingContext messagingContext)
        {
            Signing signInfo = messagingContext.SendingPMode.Security.Signing;

            if (signInfo.SigningCertificateInformation == null)
            {
                throw new ConfigurationErrorsException("No signing certificate information found in PMode to perform signing.");
            }

            var certFindCriteria = signInfo.SigningCertificateInformation as CertificateFindCriteria;

            if (certFindCriteria != null)
            {
                return _repository.GetCertificate(certFindCriteria.CertificateFindType, certFindCriteria.CertificateFindValue);
            }

            var embeddedCertInfo = signInfo.SigningCertificateInformation as PrivateKeyCertificate;

            if (embeddedCertInfo != null)
            {
                return new X509Certificate2(Convert.FromBase64String(embeddedCertInfo.Certificate), embeddedCertInfo.Password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            }

            throw new NotSupportedException("The signing certificate information specified in the PMode could not be used to retrieve the certificate");
        }

        private static ICalculateSignatureStrategy CreateSignStrategy(MessagingContext messagingContext, X509Certificate2 certificate)
        {
            AS4Message message = messagingContext.AS4Message;
            Signing signing = messagingContext.SendingPMode.Security.Signing;

            var config = new CalculateSignatureConfig(certificate,
                                                      signing.KeyReferenceMethod,
                                                      signing.Algorithm,
                                                      signing.HashFunction);

            return CalculateSignatureStrategy.ForAS4Message(message, config);
        }
    }
}