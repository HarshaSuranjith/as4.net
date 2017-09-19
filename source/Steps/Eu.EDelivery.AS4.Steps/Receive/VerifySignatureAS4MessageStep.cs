﻿using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Signing;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a <see cref="AS4Message"/> signature gets verified
    /// </summary>
    [Description("Verifies if the signature of the AS4 Message is correct. Message verification is necessary to ensure that the authenticity of the message is intact.")]
    [Info("Verify signature of received AS4 Message")]
    public class VerifySignatureAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start verifying the Signature of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            ReceivingProcessingMode pmode = messagingContext.ReceivingPMode;
            SigningVerification verification = pmode?.Security.SigningVerification;

            bool isMessageFailsTheRequiredSigning = verification?.Signature == Limit.Required && !messagingContext.AS4Message.IsSigned;
            bool isMessageFailedTheUnallowedSigning = verification?.Signature == Limit.NotAllowed && messagingContext.AS4Message.IsSigned;

            if (isMessageFailsTheRequiredSigning)
            {
                string description = $"Receiving PMode {pmode.Id} requires a Signed AS4 Message and the message is not";
                return InvalidSignatureResult(description, ErrorAlias.PolicyNonCompliance, messagingContext);
            }

            if (isMessageFailedTheUnallowedSigning)
            {
                string description = $"Receiving PMode {pmode.Id} doesn't allow a signed AS4 Message and the message is";
                return InvalidSignatureResult(description, ErrorAlias.PolicyNonCompliance, messagingContext);
            }

            if (MessageDoesNotNeedToBeVerified(messagingContext))
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            return await TryVerifyingSignature(messagingContext);
        }

        private static bool MessageDoesNotNeedToBeVerified(MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;

            return !as4Message.IsSigned ||
                    message.ReceivingPMode?.Security.SigningVerification.Signature == Limit.Ignored;
        }

        private static async Task<StepResult> TryVerifyingSignature(MessagingContext messagingContext)
        {
            try
            {
                return await VerifySignature(messagingContext).ConfigureAwait(false);
            }
            catch (CryptographicException exception)
            {
                Logger.Error($"An exception occured while validating the signature: {exception.Message}");
                return InvalidSignatureResult(exception.Message, ErrorAlias.FailedAuthentication, messagingContext);
            }
        }

        private static async Task<StepResult> VerifySignature(MessagingContext messagingContext)
        {
            if (!IsValidSignature(messagingContext.AS4Message))
            {
                string description = "The signature is invalid";
                Logger.Error(description);
                return InvalidSignatureResult(description, ErrorAlias.FailedAuthentication, messagingContext);
            }

            Logger.Info($"{messagingContext.EbmsMessageId} AS4 Message has a valid Signature present");

            foreach (Attachment attachment in messagingContext.AS4Message.Attachments)
            {
                attachment.ResetContentPosition();
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static bool IsValidSignature(AS4Message as4Message)
        {
            VerifyConfig options = CreateVerifyOptionsForAS4Message(as4Message);

            return as4Message.SecurityHeader.Verify(options);
        }

        private static VerifyConfig CreateVerifyOptionsForAS4Message(AS4Message as4Message)
        {
            return new VerifyConfig
            {
                Attachments = as4Message.Attachments
            };
        }

        private static StepResult InvalidSignatureResult(string description, ErrorAlias errorAlias, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, errorAlias);
            return StepResult.Failed(context);
        }
    }
}