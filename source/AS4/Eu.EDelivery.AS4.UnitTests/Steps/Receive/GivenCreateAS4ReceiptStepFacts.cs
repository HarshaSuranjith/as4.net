﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="CreateAS4ReceiptStep" />
    /// </summary>
    public class GivenCreateAS4ReceiptStepFacts
    {        
        private readonly CreateAS4ReceiptStep _step;

        public GivenCreateAS4ReceiptStepFacts()
        {
            _step = new CreateAS4ReceiptStep();
            IdentifierFactory.Instance.SetContext(StubConfig.Default);
        }

        public class GivenValidArguments : GivenCreateAS4ReceiptStepFacts
        {
            [Fact]
            public async Task ThenExecuteSucceedsWithDefaultInternalMessageAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                Assert.IsType(typeof(Receipt), result.MessagingContext.AS4Message.PrimarySignalMessage);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithReceiptTypeAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                var receiptMessage = result.MessagingContext.AS4Message.PrimarySignalMessage as Receipt;
                Assert.IsType(typeof(Receipt), receiptMessage);
                Assert.Null(receiptMessage.NonRepudiationInformation);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithSigningReceiptAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                Assert.False(result.MessagingContext.AS4Message.IsSigned);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNRRFormatAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateSignedInternalMessage();
                messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                var receiptMessage = result.MessagingContext.AS4Message.PrimarySignalMessage as Receipt;
                Assert.IsType(typeof(Receipt), receiptMessage);
                Assert.NotNull(receiptMessage.NonRepudiationInformation);
                Assert.Null(receiptMessage.UserMessage);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithSameReferenceTagsAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateSignedInternalMessage();
                messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                var receiptMessage = result.MessagingContext.AS4Message.PrimarySignalMessage as Receipt;
                SecurityHeader securityHeader = messagingContext.AS4Message.SecurityHeader;
                Assert.NotNull(receiptMessage);
                Assert.NotNull(securityHeader);
                AssertSignedReferences(receiptMessage, securityHeader);
            }

            private static void AssertSignedReferences(Receipt receiptMessage, SecurityHeader securityHeader)
            {
                ArrayList cryptoReferences = securityHeader.GetReferences();
                IEnumerable<Reference> receiptReferences =
                    receiptMessage.NonRepudiationInformation.MessagePartNRInformation.Select(i => i.Reference);

                foreach (CryptoReference cryptoRef in cryptoReferences)
                {
                    Reference reference = receiptReferences.FirstOrDefault(r => r.URI.Equals(cryptoRef.Uri));
                    Assert.NotNull(reference);
                }
            }
        }

        protected ReceivingProcessingMode GetReceivingPMode()
        {
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReceiptHandling.UseNRRFormat = false;
            return pmode;
        }

        protected MessagingContext CreateDefaultInternalMessage()
        {
            return new MessagingContext(AS4Message.Create(GetUserMessage()), MessagingContextMode.Receive)
            {
                ReceivingPMode = GetReceivingPMode()
            };
        }

        protected MessagingContext CreateSignedInternalMessage()
        {
            MessagingContext messagingContext = CreateDefaultInternalMessage();
            AS4Message as4Message = messagingContext.AS4Message;

            AS4MessageUtils.SignWithCertificate(as4Message, new StubCertificateRepository().GetStubCertificate());

            return messagingContext;
        }

        protected UserMessage GetUserMessage()
        {
            return new UserMessage("message-id");
        }
    }
}