﻿using System;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;

namespace Eu.EDelivery.AS4.Factories
{
    /// <summary>
    /// Factory to create <see cref="UserMessage"/> Models
    /// </summary>
    public class UserMessageFactory
    {

        public static readonly UserMessageFactory Instance = new UserMessageFactory();

        /// <summary>
        /// Create default <see cref="UserMessage"/>
        /// </summary>
        /// <returns></returns>
        public UserMessage Create(SendingProcessingMode pmode)
        {
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            var result = new UserMessage
            {
                Sender = PModePartyResolver.ResolveSender(pmode.MessagePackaging?.PartyInfo?.FromParty),
                Receiver = PModePartyResolver.ResolveReceiver(pmode.MessagePackaging?.PartyInfo?.ToParty),
                CollaborationInfo = ResolveCollaborationInfo(pmode)
            };

            if (pmode.MessagePackaging?.MessageProperties != null)
            {
                foreach (var p in pmode.MessagePackaging?.MessageProperties)
                {
                    result.AddMessageProperty(
                        new Model.Core.MessageProperty(
                            p.Name, 
                            p.Value, 
                            p.Type));
                }
            }

            return result;
        }

        private static CollaborationInfo ResolveCollaborationInfo(SendingProcessingMode pmode)
        {
            return new CollaborationInfo(
                PModeAgreementRefResolver.ResolveAgreementReference(pmode),
                PModeServiceResolver.ResolveService(pmode),
                PModeActionResolver.ResolveAction(pmode),
                CollaborationInfo.DefaultConversationId);
        }
    }
}
