﻿using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Agreement Ref is equal to the UserMessage Agreement Ref
    /// </summary>
    internal class PModeAgreementRefRule : IPModeRule
    {
        private const int Points = 4;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return IsAgreementRefEqual(pmode, userMessage) ? Points : NotEqual;
        }

        private static bool IsAgreementRefEqual(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            Model.PMode.AgreementReference pmodeAgreementRef = pmode.MessagePackaging.CollaborationInfo?.AgreementReference;
            Model.Core.AgreementReference userMessageAgreementRef = userMessage.CollaborationInfo?.AgreementReference?.GetOrElse(() => null);

            if (userMessageAgreementRef == null)
            {
                return false;
            }

            bool equalPModeId =
                (pmodeAgreementRef?.PModeId != null)
                .ThenMaybe(pmodeAgreementRef?.PModeId)
                .Equals(userMessageAgreementRef?.PModeId);

            bool equalType =
                (pmodeAgreementRef?.Type != null)
                .ThenMaybe(pmodeAgreementRef?.Type)
                .Equals(userMessageAgreementRef?.Type);

            bool areBothEqual =
                equalPModeId
                && equalType
                && pmodeAgreementRef?.Value == userMessageAgreementRef?.Value;

            return pmodeAgreementRef != null && areBothEqual;
        }
    }
}