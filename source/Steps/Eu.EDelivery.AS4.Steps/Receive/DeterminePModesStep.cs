﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using NLog;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;
using SendPMode = Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Step which describes how the PModes (Sending and Receiving) is determined
    /// </summary>
    public class DeterminePModesStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfig _config;        
        private readonly IPModeRuleVisitor _visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep" /> class
        /// </summary>
        public DeterminePModesStep() : this(Config.Instance, new PModeRuleVisitor()) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep" /> class
        /// Create a Determine Receiving PMode Step
        /// with a given Data store
        /// </summary>
        /// <param name="config"> </param>
        /// <param name="visitor"> </param>
        internal DeterminePModesStep(IConfig config, IPModeRuleVisitor visitor)
        {
            _config = config;
            _visitor = visitor;
        }

        /// <summary>
        /// Start determine the Receiving Processing Mode
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message.IsSignalMessage)
            {
                return await DetermineSendingPModeForSignalMessage(messagingContext);
            }

            return await DetermineReceivingPModeForUserMessage(messagingContext);
        }

        private static async Task<StepResult> DetermineSendingPModeForSignalMessage(MessagingContext messagingContext)
        {
            SendPMode pmode = GetPModeFromDatastore(messagingContext.AS4Message);
            if (pmode == null)
            {
                string description =
                    $"Unable to retrieve Sending PMOde from Datastore for OutMessage with Id: {messagingContext.AS4Message.PrimarySignalMessage.RefToMessageId}";
                return FailedStepResult(description, messagingContext);
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task<StepResult> DetermineReceivingPModeForUserMessage(MessagingContext messagingContext)
        {
            IEnumerable<ReceivePMode> possibilities = GetPModeFromSettings(messagingContext.AS4Message);

            if (possibilities.Any() == false)
            {
                return FailedStepResult(
                    $"No Receiving PMode was found with for UserMessage with Message Id: {messagingContext.AS4Message.GetPrimaryMessageId()}",
                    messagingContext);
            }

            if (possibilities.Count() > 1)
            {
                return FailedStepResult("More than one matching Receiving PMode was found", messagingContext);
            }

            ReceivePMode pmode = possibilities.First();
            Logger.Info($"Use '{pmode.Id}' as Receiving PMode");

            messagingContext.ReceivingPMode = pmode;
            messagingContext.SendingPMode = GetReferencedSendingPMode(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static StepResult FailedStepResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorCode.Ebms0001, ErrorAlias.ProcessingModeMismatch);
            return StepResult.Failed(context);
        }

        private static SendPMode GetPModeFromDatastore(AS4Message as4Message)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                return repository.GetOutMessageData(
                    as4Message.PrimarySignalMessage.RefToMessageId,
                    m => AS4XmlSerializer.FromString<SendPMode>(m.PMode));
            }
        }

        private IEnumerable<ReceivePMode> GetPModeFromSettings(AS4Message as4Message)
        {
            List<PModeParticipant> participants = GetPModeParticipants(as4Message.PrimaryUserMessage);
            participants.ForEach(p => p.Accept(_visitor));

            PModeParticipant winner = participants.Where(p => p.Points >= 10).Max();
            return participants.Where(p => p.Points == winner?.Points).Select(p => p.PMode);
        }

        private List<PModeParticipant> GetPModeParticipants(UserMessage primaryUser)
        {
            return _config.GetReceivingPModes().Select(pmode => new PModeParticipant(pmode, primaryUser)).ToList();
        }

        private SendPMode GetReferencedSendingPMode(MessagingContext messagingContext)
        {
            if (string.IsNullOrWhiteSpace(messagingContext.ReceivingPMode.ReceiptHandling.SendingPMode))
            {
                Logger.Warn("No SendingPMode defined in ReceiptHandling of Received PMode.");
                return null;
            }

            string pmodeId = messagingContext.ReceivingPMode.ReceiptHandling.SendingPMode;
            Logger.Info("Receipt Sending PMode Id: " + pmodeId);

            return _config.GetSendingPMode(pmodeId);
        }
    }
}