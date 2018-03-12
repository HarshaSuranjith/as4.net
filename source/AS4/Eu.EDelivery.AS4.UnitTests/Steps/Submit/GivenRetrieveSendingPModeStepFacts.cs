﻿using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="RetrieveSendingPModeStep" />
    /// </summary>
    public class GivenRetrieveSendingPModeStepFacts
    {
        [Fact]
        public async Task FailsToRetrievePMode_IfInvalidPMode()
        {
            // Arrange
            const string pmodeId = "01-pmode";
            var internalMessage = new MessagingContext(GetStubSubmitMessage(pmodeId));

            SendingProcessingMode invalidPMode = ValidSendingPModeFactory.Create(pmodeId);
            invalidPMode.ReceiptHandling.NotifyMessageProducer = true;
            invalidPMode.ReceiptHandling.NotifyMethod = null;

            var sut = new RetrieveSendingPModeStep(CreateStubConfigWithSendingPMode(invalidPMode));

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => sut.ExecuteAsync(internalMessage));
        }

        private static SubmitMessage GetStubSubmitMessage(string pmodeId)
        {
            return new SubmitMessage
            {
                Collaboration = new CollaborationInfo {AgreementRef = new Agreement {PModeId = pmodeId}}
            };
        }

        private static IConfig CreateStubConfigWithSendingPMode(SendingProcessingMode pmode)
        {
            var stubConfig = new Mock<IConfig>();
            stubConfig.Setup(c => c.GetSendingPMode(It.IsAny<string>())).Returns(pmode);

            return stubConfig.Object;
        }
    }
}