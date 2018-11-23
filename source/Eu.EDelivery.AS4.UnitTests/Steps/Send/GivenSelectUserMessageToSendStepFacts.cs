﻿using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Mappings.PMode;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SelectUserMessageToSendStep"/>
    /// </summary>
    public class GivenSelectUserMessageToSendStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task SelectionReturnsPullRequestWarning_IfNoMatchesAreFound()
        {
            // Act
            StepResult result = await ExerciseSelection(expectedMpc: null);

            // Assert
            var signal = Assert.IsType<Error>(result.MessagingContext.AS4Message.FirstSignalMessage);
            Assert.True(signal.IsPullRequestWarning, "error signal is not a warning for a pull request");
            Assert.False(result.CanProceed);
        }

        [Fact]
        public async Task SelectsUserMessage_IfUserMessageMatchesCriteria()
        {
            // Arrange
            const string expectedMpc = "message-mpc";
            InsertUserMessage(expectedMpc, MessageExchangePattern.Push, Operation.ToBeSent);
            InsertUserMessage("yet-another-mpc", MessageExchangePattern.Pull, Operation.DeadLettered);
            InsertUserMessage(expectedMpc, MessageExchangePattern.Pull, Operation.ToBeSent);

            // Act
            StepResult result = await ExerciseSelection(expectedMpc);

            // Assert

            AS4Message as4Message = await RetrieveAS4MessageFromContext(result.MessagingContext);

            UserMessage userMessage = as4Message.FirstUserMessage;

            Assert.Equal(expectedMpc, userMessage.Mpc);
            AssertOutMessage(userMessage.MessageId, m => Assert.True(m.Operation == Operation.Sent));
            Assert.NotNull(result.MessagingContext.SendingPMode);
        }

        private static async Task<AS4Message> RetrieveAS4MessageFromContext(MessagingContext context)
        {
            if (context.AS4Message != null)
            {
                return context.AS4Message;
            }

            if (context.ReceivedMessage == null)
            {
                throw new InvalidOperationException("A ReceivedMessage was expected in the MessagingContext.");
            }

            context.ReceivedMessage.UnderlyingStream.Position = 0;

            return await SerializerProvider
                .Default
                .Get(context.ReceivedMessage.ContentType)
                .DeserializeAsync(
                    context.ReceivedMessage.UnderlyingStream, 
                    context.ReceivedMessage.ContentType);
        }

        private async Task<StepResult> ExerciseSelection(string expectedMpc)
        {
            var sut = new SelectUserMessageToSendStep(GetDataStoreContext, InMemoryMessageBodyStore.Default);
            MessagingContext context = ContextWithPullRequest(expectedMpc);

            // Act
            return await sut.ExecuteAsync(context);
        }

        private void InsertUserMessage(string mpc, MessageExchangePattern pattern, Operation operation)
        {
            var sendingPMode = new SendingProcessingMode()
            {
                Id = "SomePModeId",
                MessagePackaging = { Mpc = mpc }
            };

            UserMessage userMessage = SendingPModeMap.CreateUserMessage(sendingPMode);
            AS4Message as4Message = AS4Message.Create(userMessage, sendingPMode);

            var om = new OutMessage(userMessage.MessageId)
            {
                MEP = pattern,
                Mpc = mpc,
                ContentType = as4Message.ContentType,
                EbmsMessageType = MessageType.UserMessage,
                Operation = operation,
                MessageLocation =
                    InMemoryMessageBodyStore.Default.SaveAS4Message(location: "some-location", message: as4Message)
            };

            om.SetPModeInformation(sendingPMode);
            GetDataStoreContext.InsertOutMessage(om);
        }

        private static MessagingContext ContextWithPullRequest(string mpc)
        {
            var pullRequest = new PullRequest("message-id", mpc);
            return new MessagingContext(AS4Message.Create(pullRequest), MessagingContextMode.Send);
        }

        private void AssertOutMessage(string messageId, Action<OutMessage> assertion)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                OutMessage outMessage = context.OutMessages.First(m => m.EbmsMessageId.Equals(messageId));
                assertion(outMessage);
            }
        }
    }
}
