﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="SaveReceivedMessageStep" />
    /// </summary>
    public class GivenSaveReceivedMessageDatastoreFacts : GivenDatastoreStepFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        public GivenSaveReceivedMessageDatastoreFacts()
        {
            Step = new SaveReceivedMessageStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        [Fact]
        public async Task ThenExecuteStepIgnoresPullRequests()
        {
            // Arrange
            var pr = AS4Message.Create(
                new PullRequest(
                    $"pr-mpc-{Guid.NewGuid()}",
                    $"pr-msg-id-{Guid.NewGuid()}"));

            // Act
            using (MessagingContext ctx = 
                CreateReceivedMessagingContext(pr, new ReceivingProcessingMode()))
            {
                await Step.ExecuteAsync(ctx);
            }

            // Assert
            InMessage im = GetDataStoreContext.GetInMessage(
                m => m.EbmsMessageId == pr.GetPrimaryMessageId());
            Assert.Null(im);
        }

        [Fact]
        public async Task ThenExecuteStepSavesBothUserAndReceiptMessage()
        {
            // Arrange
            UserMessage um = CreateUserMessage();
            SignalMessage r = CreateReceipt();
            var as4 = AS4Message.Empty;
            as4.AddMessageUnit(um);
            as4.AddMessageUnit(r);

            // Act
            using (MessagingContext ctx = 
                CreateReceivedMessagingContext(as4, new ReceivingProcessingMode()))
            {
                // Act
                await Step.ExecuteAsync(ctx);
            }

            // Assert
            GetDataStoreContext.AssertInMessage(um.MessageId, Assert.NotNull);
            GetDataStoreContext.AssertInMessage(r.MessageId, Assert.NotNull);
        }

        [Fact]
        public async Task ThenExecuteStepIsTestUserMessage()
        {
            // Arrange
            UserMessage userMessage = CreateUserMessage();
            AddTestableDataToUserMessage(userMessage);

            AS4Message as4Message = AS4Message.Create(userMessage);

            var pmode = new ReceivingProcessingMode();
            pmode.Reliability.DuplicateElimination.IsEnabled = true;

            using (MessagingContext messagingContext = CreateReceivedMessagingContext(as4Message, pmode))
            {
                // Act
                await Step.ExecuteAsync(messagingContext);

                // Assert
                InMessage m = GetUserInMessageForEbmsMessageId(userMessage);
                Assert.Equal(Operation.NotApplicable, m.Operation);
            }
        }

        [Fact]
        public async Task ThenExecuteStepUpdatesDuplicateReceiptMessage()
        {
            // Arrange
            SignalMessage signalMessage = new Receipt((string) "ref-to-message-id");
            signalMessage.IsDuplicate = false;

            using (MessagingContext messagingContext =
                CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
            {
                // Act           
                // Execute the step twice.     
                StepResult stepResult = await Step.ExecuteAsync(messagingContext);
                Assert.False(stepResult.MessagingContext.AS4Message.FirstSignalMessage.IsDuplicate);
            }

            using (MessagingContext messagingContext =
                CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
            {
                StepResult stepResult = await Step.ExecuteAsync(messagingContext);

                // Assert
                Assert.True(stepResult.MessagingContext.AS4Message.FirstSignalMessage.IsDuplicate);
            }
        }

        [Fact]
        public async Task ThenExecuteStepUpdatesDuplicateUserMessage()
        {
            // Arrange
            UserMessage userMessage = CreateUserMessage();
            InsertDuplicateUserMessage(userMessage);

            var pmode = new ReceivingProcessingMode();
            pmode.Reliability.DuplicateElimination.IsEnabled = true;

            using (MessagingContext context =
                CreateReceivedMessagingContext(AS4Message.Create(userMessage), pmode))
            {
                // Act
                await Step.ExecuteAsync(context);
            }

            // Assert
            InMessage m = GetUserInMessageForEbmsMessageId(userMessage);
            Assert.Equal(Operation.NotApplicable, m.Operation);
        }

        private void InsertDuplicateUserMessage(MessageUnit userMessage)
        {
            GetDataStoreContext.InsertInMessage(new InMessage(ebmsMessageId: userMessage.MessageId));
        }

        private InMessage GetUserInMessageForEbmsMessageId(MessageUnit userMessage)
        {
            InMessage inMessage = GetDataStoreContext
                .GetInMessage(m => m.EbmsMessageId.Equals(userMessage.MessageId));

            Assert.NotNull(inMessage);
            Assert.Equal(MessageType.UserMessage, inMessage.EbmsMessageType);

            return inMessage;
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsAsync()
        {
            // Arrange
            using (MessagingContext context =
                CreateReceivedMessagingContext(AS4Message.Empty, receivingPMode: null))
            {
                // Act
                StepResult result = await Step.ExecuteAsync(context);

                // Assert
                Assert.NotNull(result);
            }
        }

        private static UserMessage CreateUserMessage()
        {
            string userMessageId = Guid.NewGuid().ToString();
            return new UserMessage(userMessageId) { RefToMessageId = userMessageId };
        }

        private static void AddTestableDataToUserMessage(UserMessage userMessage)
        {
            userMessage.CollaborationInfo = 
                new CollaborationInfo(
                    userMessage.CollaborationInfo.AgreementReference, 
                    Service.TestService,
                    Constants.Namespaces.TestAction,
                    userMessage.CollaborationInfo.ConversationId);
        }

        protected MessagingContext CreateReceivedMessagingContext(AS4Message as4Message, ReceivingProcessingMode receivingPMode)
        {
            var stream = new MemoryStream();

            SerializerProvider
                .Default
                .Get(as4Message.ContentType)
                .Serialize(as4Message, stream, CancellationToken.None);

            stream.Position = 0;

            var ctx = new MessagingContext(
                    new ReceivedMessage(stream, as4Message.ContentType),
                    MessagingContextMode.Receive)
                { ReceivingPMode = receivingPMode };

            ctx.ModifyContext(as4Message);
            return ctx;
        }
    }
}