using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Parties are equal to the UserMessage Parties
    /// </summary>
    internal class PModePartyInfoRule : IPModeRule
    {        
        private const int PartyToPoints = 8;
        private const int PartyFromPoints = 7;
        private const int PartyRolePoints = 1;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            if (pmode.MessagePackaging.PartyInfo == null)
            {
                return NotEqual;
            }

            var pmodePartyInfo = pmode.MessagePackaging.PartyInfo;

            if (pmodePartyInfo.IsEmpty())
            {
                return NotEqual;
            }

            int points = NotEqual;

            if (IsPartyInfoEqual(pmodePartyInfo.FromParty, userMessage.Sender))
            {
                points += PartyFromPoints;
            }

            if (IsPartyInfoEqual(pmodePartyInfo.ToParty, userMessage.Receiver))
            {
                points += PartyToPoints;
            }

            if (IsPartyInfoRoleEqual(pmodePartyInfo, userMessage))
            {
                points += PartyRolePoints;
            }

            return points;
        }        

        private static bool IsPartyInfoEqual(Party pmodeParty, Party messageParty)
        {
            if (pmodeParty == null || messageParty == null)
            {
                return false;
            }

            return pmodeParty.PartyIds.All(messageParty.PartyIds.Contains);
        }

        private static bool IsPartyInfoRoleEqual(PartyInfo pmodePartyInfo, UserMessage userMessage)
        {
            if (userMessage.Sender == null || userMessage.Receiver == null)
            {
                return false;
            }

            return pmodePartyInfo.FromParty.Role.Equals(userMessage.Sender.Role, StringComparison.OrdinalIgnoreCase) &&
                   pmodePartyInfo.ToParty.Role.Equals(userMessage.Receiver.Role, StringComparison.OrdinalIgnoreCase);
        }

    }
}