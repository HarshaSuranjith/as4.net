﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep" /> implementation
    /// to dynamically complete the <see cref="SendingProcessingMode"/>
    /// </summary>    
    [Info("Perform Dynamic Discovery if required")]
    [Description("Contacts an SMP server and executes the configured SMP Profile if dynamic discovery is enabled. \n\r" +
        "The information returned from the SMP server is used to complete the sending PMode.")]
    public class DynamicDiscoveryStep : IStep
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.SendingPMode.DynamicDiscoverySpecified == false)
            {
                return StepResult.Success(messagingContext);
            }

            string smpProfile = messagingContext.SendingPMode.DynamicDiscovery.SmpProfile;
            Logger.Info($"DynamicDiscovery is enabled in Sending PMode - using {smpProfile}");

            var clonedPMode = (SendingProcessingMode)messagingContext.SendingPMode.Clone();
            clonedPMode.Id = $"{clonedPMode.Id}_SMP";

            var profile = GenericTypeBuilder.FromType(smpProfile).Build<IDynamicDiscoveryProfile>();
            XmlDocument smpMetaData = await RetrieveSmpMetaData(profile, clonedPMode);

            SendingProcessingMode sendingPMode = profile.DecoratePModeWithSmpMetaData(clonedPMode, smpMetaData);
            Logger.Info("Sending PMode completed with SMP metadata");
            ValidatePMode(sendingPMode);

            messagingContext.SendingPMode = sendingPMode;
            messagingContext.SubmitMessage.PMode = sendingPMode;
            return StepResult.Success(messagingContext);
        }

        private static async Task<XmlDocument> RetrieveSmpMetaData(
            IDynamicDiscoveryProfile profile, 
            SendingProcessingMode clonedPMode)
        {
            if (clonedPMode.DynamicDiscovery == null)
            {
                throw new ConfigurationErrorsException(@"The Sending PMode requires a DynamicDiscovery element");
            }

            if (clonedPMode.MessagePackaging?.PartyInfo?.ToParty?.PartyIds.Any() != true)
            {
                throw new ConfigurationErrorsException("The Sending PMode must contain a ToParty Id");
            }

            string toPartyId = clonedPMode.MessagePackaging.PartyInfo.ToParty.PartyIds.First().Id;

            Dictionary<string, string> customProperties = 
                clonedPMode.DynamicDiscovery.Settings?.ToDictionary(s => s.Key, s => s.Value) 
                    ?? new Dictionary<string, string>();

            return await profile.RetrieveSmpMetaData(
                partyId: toPartyId, 
                properties: customProperties);
        }

        private static void ValidatePMode(SendingProcessingMode pmode)
        {
            SendingProcessingModeValidator.Instance.Validate(pmode).Result(
                onValidationSuccess: result => Logger.Info($"Dynamically completed PMode {pmode.Id} is valid"),
                onValidationFailed: result =>
                {
                    string errorMessage = result.AppendValidationErrorsToErrorMessage($"Dynamically completed PMode {pmode.Id} was invalid:");

                    Logger.Error(errorMessage);

                    throw new ConfigurationErrorsException(errorMessage);
                });
        }
    }
}
