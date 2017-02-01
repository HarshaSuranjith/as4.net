﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
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
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IConfig _config;
        private readonly IPModeRuleVisitor _visitor;

        private AS4Message _as4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep"/> class
        /// </summary>
        public DeterminePModesStep()
        {
            this._config = Config.Instance;
            this._visitor = new PModeRuleVisitor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep"/> class
        /// Create a Determine Receiving PMode Step
        /// with a given Data store
        /// </summary>        
        /// <param name="config"> </param>
        /// <param name="visitor"> </param>
        internal DeterminePModesStep(IConfig config, IPModeRuleVisitor visitor)
        {
            this._config = config;
            this._visitor = visitor;
        }

        /// <summary>
        /// Start determine the Receiving Processing Mode
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Throws exception when a PMode cannot be retrieved</exception>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._as4Message = internalMessage.AS4Message;

            if (this._as4Message.IsSignalMessage)
                this._as4Message.SendingPMode = GetPModeFromDatastore();
            else
                this._as4Message.ReceivingPMode = GetPModeFromSettings();

            if (this._as4Message.SendingPMode?.Id == null)
                this._as4Message.SendingPMode = GetReferencedSendingPMode();

            internalMessage.AS4Message = this._as4Message;
            return StepResult.SuccessAsync(internalMessage);
        }

        private SendPMode GetPModeFromDatastore()
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                string refToMessageId = this._as4Message.PrimarySignalMessage.RefToMessageId;
                OutMessage outMessage = repository.GetOutMessageById(refToMessageId);

                if (outMessage == null)
                    throw ThrowAS4Exception(
                        $"Unable to retrieve Sending PMode from Datastore with Id: {refToMessageId}");

                var pmode = AS4XmlSerializer.Deserialize<SendPMode>(outMessage.PMode);
                this._logger.Info($"Get Sending PMode {pmode.Id} from Datastore");

                return pmode;
            }
        }

        private ReceivePMode GetPModeFromSettings()
        {
            PModeParticipant[] participants = GetPModeParticipants();
            LetParticipantsAcceptVisitor(participants);

            PModeParticipant winningParticipant = participants.Where(p => p.Points >= 10).Max();
            PostConditionsWiningParticipant(participants, winningParticipant);

            this._logger.Info(
                $"Using Receiving PMode {winningParticipant.PMode.Id} with {winningParticipant.Points} Points");
            return winningParticipant.PMode;
        }

        private PModeParticipant[] GetPModeParticipants()
        {
            return this._config
                .GetReceivingPModes()
                .Select(CreateParticipant)
                .ToArray();
        }

        private PModeParticipant CreateParticipant(ReceivePMode pmode)
        {
            return new PModeParticipant(pmode, this._as4Message.PrimaryUserMessage);
        }

        private void LetParticipantsAcceptVisitor(IEnumerable<PModeParticipant> participants)
        {
            foreach (PModeParticipant participant in participants)
                LetParticipantAcceptVisitor(participant);
        }

        private void LetParticipantAcceptVisitor(PModeParticipant participant)
        {
            participant.Accept(this._visitor);
            this._logger.Debug($"Receiving PMode: {participant.PMode.Id} has {participant.Points} Points");
        }

        private void PostConditionsWiningParticipant(
            IEnumerable<PModeParticipant> participants, PModeParticipant winningParticipant)
        {
            if (winningParticipant == null)
                throw ThrowAS4Exception(
                    $"No Receiving PMode was found with for UserMessage with Message Id: {this._as4Message.PrimaryUserMessage.MessageId}");

            if (TheresMoreThanOwnWinningParticipant(participants, winningParticipant))
            {
                throw ThrowToManyPModeFoundException();
            }
        }

        private static bool TheresMoreThanOwnWinningParticipant(
            IEnumerable<PModeParticipant> participants, PModeParticipant winningParticipant)
        {
            return participants.Count(p => p.Points == winningParticipant.Points) > 1;
        }

        private AS4Exception ThrowToManyPModeFoundException()
        {
            const string description = "More than one matching PMode was found";
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithMessageIds(this._as4Message.MessageIds)
                .Build();
        }

        private SendPMode GetReferencedSendingPMode()
        {
            string pmodeId = this._as4Message.ReceivingPMode.ReceiptHandling.SendingPMode;
            this._logger.Info("Receipt Sending PMode Id: " + pmodeId);
            return TryGetSendingPMode(pmodeId);
        }

        private SendPMode TryGetSendingPMode(string pmodeId)
        {
            try
            {
                return this._config.GetSendingPMode(pmodeId);
            }
            catch (Exception)
            {
                throw ThrowAS4Exception("Receiving PMode references a non-existing Sending PMode");
            }
        }

        private AS4Exception ThrowAS4Exception(string description)
        {
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0001)
                .WithMessageIds(this._as4Message.MessageIds)
                .Build();
        }
    }
}