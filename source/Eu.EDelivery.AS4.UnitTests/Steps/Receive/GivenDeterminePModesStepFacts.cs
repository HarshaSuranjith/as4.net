﻿using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Moq;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="DeterminePModesStep" />
    /// </summary>
    public class GivenDeterminePModesStepFacts : GivenDatastoreFacts
    {
        private readonly Mock<IConfig> _mockedConfig;
        private readonly DeterminePModesStep _step;

        public GivenDeterminePModesStepFacts()
        {
            _mockedConfig = new Mock<IConfig>();
            _step = new DeterminePModesStep(_mockedConfig.Object, GetDataStoreContext);
        }

        public class GivenValidArguments : GivenDeterminePModesStepFacts
        {
            [Fact]
            public async Task Determine_Both_Sending_And_Receiving_PMode_When_Bundled()
            {
                // Arrange
                var nonMultihopSignal = new Receipt($"reftoid-{Guid.NewGuid()}");

                string receivePModeId = $"receive-pmodeid-{Guid.NewGuid()}";
                var userMesssage = new UserMessage(
                    messageId: $"user-{Guid.NewGuid()}",
                    collaboration: new AS4.Model.Core.CollaborationInfo(
                        new AgreementReference(
                            "agreement",
                            receivePModeId),
                        Service.TestService,
                        Constants.Namespaces.TestAction,
                        AS4.Model.Core.CollaborationInfo.DefaultConversationId));

                string sendPModeId = $"send-pmodeid-{Guid.NewGuid()}";
                var expected = new SendingProcessingMode { Id = sendPModeId };
                InsertOutMessage(nonMultihopSignal.RefToMessageId, expected);

                var msg = AS4Message.Create(userMesssage);
                msg.AddMessageUnit(nonMultihopSignal);

                // Act
                StepResult result = await ExerciseDeterminePModes(
                    msg, 
                    new ReceivePMode
                    {
                        Id = receivePModeId,
                        ReplyHandling = new ReplyHandling { SendingPMode = "some-other-send-pmodeid" }
                    });

                // Assert
                Assert.Equal(
                    receivePModeId,
                    result.MessagingContext.ReceivingPMode.Id);
                Assert.Equal(
                    sendPModeId,
                    result.MessagingContext.SendingPMode.Id);
            }

            [Fact]
            public async Task Dont_Use_Scoring_System_ReceivingPMode_When_Already_Configure()
            {
                // Arrange
                var expected = new ReceivePMode { Id = "static-receive-configured" };

                // Act
                StepResult result = await _step.ExecuteAsync(
                    new MessagingContext(
                        AS4Message.Empty, 
                        MessagingContextMode.Receive)
                    {
                        ReceivingPMode = expected
                    });

                // Assert
                Assert.Same(
                    expected,
                    result.MessagingContext.ReceivingPMode);
            }

            [Fact]
            public async Task SendingPModeIsFound_IfSignalMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                var expected = new SendingProcessingMode { Id = Guid.NewGuid().ToString() };
                InsertOutMessage(messageId, expected);

                AS4Message as4Message = AS4Message.Create(new Receipt(messageId));

                // Act
                StepResult result = await ExerciseDeterminePModes(as4Message);

                // Assert
                SendingProcessingMode actual = result.MessagingContext.SendingPMode;
                Assert.Equal(expected.Id, actual.Id);
            }

            private void InsertOutMessage(string messageId, SendingProcessingMode pmode)
            {
                var outMessage = new OutMessage(ebmsMessageId: messageId);
                outMessage.SetPModeInformation(pmode);

                GetDataStoreContext.InsertOutMessage(outMessage);
            }

            private async Task<StepResult> ExerciseDeterminePModes(AS4Message message, params ReceivePMode[] pmodes)
            {
                var stubConfig = new Mock<IConfig>();
                stubConfig.Setup(c => c.GetReceivingPModes()).Returns(pmodes);
                var sut = new DeterminePModesStep(stubConfig.Object, GetDataStoreContext);

                return await sut.ExecuteAsync(
                    new MessagingContext(message, MessagingContextMode.Receive));
            }
        }

        /// <summary>
        /// Testing the step with invalid arguments
        /// </summary>
        public class GivenInvalidArguments : GivenDeterminePModesStepFacts
        {
            [Theory]
            [InlineData("action", "service")]
            public async Task ThenServiceAndActionIsNotEnoughAsync(string action, string service)
            {
                // Arrange
                ArrangePModeThenServiceAndActionIsNotEnough(action, service);

                var userMessage = new UserMessage(
                    Guid.NewGuid().ToString(),
                    new AS4.Model.Core.CollaborationInfo(
                        Maybe<AgreementReference>.Nothing,
                        new Service(service),
                        action,
                        "1"));

                var messagingContext = new MessagingContext(AS4Message.Create(userMessage), MessagingContextMode.Receive);

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                Assert.False(result.Succeeded);
                ErrorResult errorResult = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0010, errorResult.Code);
            }

            private void ArrangePModeThenServiceAndActionIsNotEnough(string action, string service)
            {
                ReceivePMode pmode = CreatePModeWithActionService(service, action);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                DifferentiatePartyInfo(pmode);
                SetupPModes(pmode, new ReceivePMode() { Id = "other id", ReplyHandling = {SendingPMode = "other pmode"}});
            }

            [Theory]
            [InlineData("name", "type")]
            public async Task ThenAgreementRefIsNotEnoughAsync(string name, string type)
            {
                // Arrange
                var agreementRef = new AS4.Model.PMode.AgreementReference { Value = name, Type = type, PModeId = "pmode-id" };
                ArrangePModeThenAgreementRefIsNotEnough(agreementRef);

                var userMessage = new UserMessage(
                    Guid.NewGuid().ToString(),
                    new AS4.Model.Core.CollaborationInfo(
                        agreement: new AgreementReference(name, type, "pmode-id"),
                        service: new Service("service"), 
                        action: "action",
                        conversationId: "1"));

                var messagingContext = new MessagingContext(AS4Message.Create(userMessage), MessagingContextMode.Receive);

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                Assert.False(result.Succeeded);
                ErrorResult errorResult = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0010, errorResult.Code);
            }

            private void ArrangePModeThenAgreementRefIsNotEnough(AS4.Model.PMode.AgreementReference agreementRef)
            {
                ReceivePMode pmode = CreatePModeWithAgreementRef(agreementRef);
                DifferentiatePartyInfo(pmode);
                SetupPModes(pmode, new ReceivePMode());
            }
        }

        protected ReceivePMode CreateDefaultPMode(string id)
        {
            return new ReceivePMode
            {
                Id = id,
                MessagePackaging = new MessagePackaging
                {
                    CollaborationInfo = new CollaborationInfo(),
                    PartyInfo = new PartyInfo()
                },
                ReplyHandling = { SendingPMode = "response_pmode" }
            };
        }

        protected void SetupPModes(params ReceivePMode[] pmodes)
        {
            _mockedConfig.Setup(c => c.GetReceivingPModes()).Returns(pmodes);
        }

        protected ReceivePMode CreatePModeWithAgreementRef(AS4.Model.PMode.AgreementReference agreementRef)
        {
            ReceivePMode pmode = CreateDefaultPMode("defaultPMode");
            pmode.MessagePackaging.CollaborationInfo.AgreementReference = agreementRef;

            return pmode;
        }

        protected ReceivePMode CreatePModeWithActionService(string service, string action)
        {
            ReceivePMode pmode = CreateDefaultPMode("defaultPMode");
            pmode.MessagePackaging.CollaborationInfo.Action = action;
            pmode.MessagePackaging.CollaborationInfo.Service.Value = service;

            return pmode;
        }

        protected void AssertPMode(ReceivePMode expectedPMode, StepResult result)
        {
            Assert.NotNull(expectedPMode);
            Assert.NotNull(result);
            Assert.Equal(expectedPMode, result.MessagingContext.ReceivingPMode);
        }

        private static void DifferentiatePartyInfo(ReceivePMode pmode)
        {
            const string fromId = "from-Id";
            const string toId = "to-Id";

            var fromParty = new Party { Role = fromId, PartyIds = { new PartyId { Id = fromId } } };
            var toParty = new Party { Role = toId, PartyIds = { new PartyId { Id = toId } } };

            pmode.MessagePackaging.PartyInfo = new PartyInfo { FromParty = fromParty, ToParty = toParty };
        }
    }
}