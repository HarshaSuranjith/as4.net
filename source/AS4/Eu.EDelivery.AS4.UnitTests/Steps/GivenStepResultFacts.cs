﻿using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// Testing <see cref="StepResult" />
    /// </summary>
    public class GivenStepResultFacts
    {
        public class CanContinue
        {
            [Fact]
            public void IsFalseIfStopExecutionIsCalled()
            {
                // Arrange
                AS4Exception exception = AS4ExceptionBuilder.WithDescription("ignored message").Build();

                // Act
                StepResult actualStepResult = StepResult.Failed(exception).AndStopExecution();

                // Assert
                Assert.False(actualStepResult.CanExecute);
            }

            [Fact]
            public void IsTrueIfStopExecutiongIsntCalled()
            {
                // Arrange
                AS4Message as4Message = new AS4MessageBuilder().Build();

                // Act
                StepResult actualStepResult = StepResult.Success(new InternalMessage(as4Message));

                // Assert
                Assert.True(actualStepResult.CanExecute);
            }
        }
    }
}