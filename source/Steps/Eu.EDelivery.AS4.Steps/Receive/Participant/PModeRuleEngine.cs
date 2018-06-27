﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive.Participant
{
    /// <summary>
    /// Class to Provide <see cref="IPModeRule" /> implementations
    /// </summary>
    internal static class PModeRuleEngine
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ICollection<IPModeRule> Rules;

        static PModeRuleEngine()
        {
            Rules = new Collection<IPModeRule>
            {
                new PModeIdRule(),
                new PModePartyInfoRule(),
                new PModeUndefinedPartyInfoRule(),
                new PModeAgreementRefRule(),
                new PModeServiceActionRule()
            };
        }

        /// <summary>
        /// Visits the <see cref="PModeParticipant" />:
        /// apply Rules on the Participant
        /// </summary>
        /// <param name="participant"></param>
        public static PModeParticipant ApplyRules(PModeParticipant participant)
        {
            foreach (IPModeRule rule in Rules)
            {
                int points = rule.DeterminePoints(participant.PMode, participant.UserMessage);
                Logger.Trace($"PMode {participant.PMode.Id}: {points} Points determined for the {rule.GetType().Name}");

                participant.Points += points;
            }

            return participant;
        }
    }  
}