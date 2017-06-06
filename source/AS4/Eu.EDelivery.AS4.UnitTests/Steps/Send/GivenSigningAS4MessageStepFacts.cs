﻿using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SignAS4MessageStep" />
    /// </summary>
    public class GivenSigningAS4MessageStepFacts
    {
        private readonly SignAS4MessageStep _step;

        public GivenSigningAS4MessageStepFacts()
        {
            var mockedContext = new Mock<IConfig>();
            _step = new SignAS4MessageStep(new CertificateRepository(mockedContext.Object));
        }

        /// <summary>
        /// Testing if the SigningTransmitter succeeds for the "Execute" Method
        /// </summary>
        public class GivenValidArgumentsExecute : GivenSigningAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenMessageDontGetSignedWhenItsDisabledAsync()
            {
                // Arrange
                var internalMessage = new MessagingContext(new AS4Message()) {SendingPMode = new SendingProcessingMode()};
                internalMessage.SendingPMode.Security.Signing.IsEnabled = false;

                // Act
                StepResult stepResult = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                SecurityHeader securityHeader = stepResult.MessagingContext.AS4Message.SecurityHeader;
                Assert.NotNull(securityHeader);
                Assert.False(securityHeader.IsSigned);
                Assert.False(securityHeader.IsEncrypted);
            }
        }
    }
}