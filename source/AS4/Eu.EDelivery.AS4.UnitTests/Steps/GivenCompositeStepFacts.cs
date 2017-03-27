﻿using System;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// Testing <seealso cref="CompositeStep" />
    /// </summary>
    public class GivenCompositeStepFacts
    {
        /// <summary>
        /// Testing if the transmitter succeeds
        /// </summary>
        public class GivenCompositeStepSucceeds : GivenCompositeStepFacts
        {
            [Fact]
            public async void ThenTransmitMessageSucceeds()
            {
                // Arrange
                InternalMessage dummyMessage = CreateDummyMessage();
                StepResult expectedStepResult = StepResult.Success(dummyMessage);

                var compositeStep = new CompositeStep(CreateMockStepWith(expectedStepResult).Object);

                // Act
                StepResult actualStepResult = await compositeStep.ExecuteAsync(dummyMessage, CancellationToken.None);

                // Assert
                Assert.Equal(expectedStepResult.InternalMessage, actualStepResult.InternalMessage);
            }

            [Fact]
            public async void ThenStepStopExecutionWithMarkedStepResult()
            {
                // Arrange
                InternalMessage expectedMessage = CreateDummyMessage();
                StepResult stopExecutionResult = StepResult.Success(expectedMessage).AndStopExecution();

                var spyStep = new SpyStep();
                var compositeStep = new CompositeStep(CreateMockStepWith(stopExecutionResult).Object, spyStep);

                // Act
                StepResult actualResult = await compositeStep.ExecuteAsync(new InternalMessage(), CancellationToken.None);

                // Assert  
                Assert.False(spyStep.IsCalled);
                Assert.Equal(expectedMessage, actualResult.InternalMessage);
            }

            private static InternalMessage CreateDummyMessage()
            {
                return new InternalMessage(new AS4MessageBuilder().WithAttachment(new Attachment()).Build());
            }

            private static Mock<IStep> CreateMockStepWith(StepResult stepResult)
            {
                var mockStep = new Mock<IStep>();

                mockStep.Setup(m => m.ExecuteAsync(It.IsAny<InternalMessage>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(stepResult);

                return mockStep;
            }
        }

        /// <summary>
        /// Testing if the transmitter fails
        /// </summary>
        public class GivenCompositeStepFails : GivenCompositeStepFacts
        {
            [Fact]
            public void ThenCreatingTransmitterFails()
            {
                // Act / Assert
                Assert.Throws<ArgumentNullException>(() => new CompositeStep(steps: null));
            }
        }
    }
}