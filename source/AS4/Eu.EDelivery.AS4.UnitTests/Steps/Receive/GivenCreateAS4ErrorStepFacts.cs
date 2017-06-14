﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="CreateAS4ErrorStep" />
    /// </summary>
    public class GivenCreateAS4ErrorStepFacts : GivenDatastoreFacts
    {
        public GivenCreateAS4ErrorStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenCreateAS4ErrorStepFacts
        {
            public IStep Step => new CreateAS4ErrorStep(new StubMessageBodyStore(), () => new DatastoreContext(Options));

            [Fact]
            public async Task ThenNotApplicableIfMessageIsEmptySoapBodyAsync()
            {
                // Arrange
                var internalMessage = new MessagingContext(exception: null);

                // Act
                StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.Equal(internalMessage, result.MessagingContext);
            }

            [Fact]
            public async Task ThenErrorIsCreatedWithAS4ExceptionAsync()
            {
                // Arrange
                var internalMessage = new MessagingContext(CreateFilledAS4Message())
                {
                    Exception = CreateFilledAS4Exception(),
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };
                
                // Act
                StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                var error = result.MessagingContext.AS4Message.PrimarySignalMessage as Error;
                Assert.NotNull(error);
                Assert.Equal("message-id", error.Exception.MessageIds.FirstOrDefault());
                Assert.Equal(ErrorCode.Ebms0001, error.Exception.ErrorCode);
            }

            [Fact]
            public async Task ThenErrorIsCreatedWithPModesAsync()
            {
                // Arrange
                var internalMessage = new MessagingContext(CreateFilledAS4Message())
                {
                    Exception = CreateFilledAS4Exception(),
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };

                // Act
                StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.Equal(internalMessage.ReceivingPMode, result.MessagingContext.ReceivingPMode);
                Assert.Equal(internalMessage.SendingPMode, result.MessagingContext.SendingPMode);
            }

            [Fact]
            public async Task ThenErrorIsCreatedWithSigningIdAsync()
            {
                // Arrange
                AS4Message as4Message = CreateFilledAS4Message();
                as4Message.SigningId = new SigningId("header-id", "body-id");

                var internalMessage = new MessagingContext(as4Message)
                {
                    Exception = CreateFilledAS4Exception(),
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };

                // Act
                StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.Equal(as4Message.SigningId, result.MessagingContext.AS4Message.SigningId);
            }

            private static AS4Exception CreateFilledAS4Exception()
            {
                return
                    AS4ExceptionBuilder.WithDescription("Testing AS4 Exception")
                                       .WithErrorCode(ErrorCode.Ebms0001)
                                       .WithMessageIds("message-id")
                                       .Build();
            }

            private static AS4Message CreateFilledAS4Message()
            {
                return AS4Message.Create(new UserMessage("message-id"));
            }
        }
    }
}