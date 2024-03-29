﻿using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Moq;
using Xunit;
using RetryReliability = Eu.EDelivery.AS4.Entities.RetryReliability;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="SendDeliverMessageStep" />
    /// </summary>
    public class GivenSendDeliverMessageStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ThenExecuteStepFailsWithFailedSenderAsync()
        {
            // Arrange
            DeliverMessageEnvelope envelope = EmptyDeliverMessageEnvelope();
            IStep sut = CreateSendDeliverStepWithSender(new SaboteurSender());

            // Act
            await Assert.ThrowsAnyAsync<Exception>(() => sut.ExecuteAsync(new MessagingContext(envelope)));
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsWithValidSenderAsync()
        {
            // Arrange
            DeliverMessageEnvelope envelope = EmptyDeliverMessageEnvelope();

            var spySender = new Mock<IDeliverSender>();
            spySender.Setup(s => s.SendAsync(envelope))
                     .ReturnsAsync(SendResult.Success);

            IStep sut = CreateSendDeliverStepWithSender(spySender.Object);

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope) { ReceivingPMode = CreateDefaultReceivingPMode() });

            // Assert
            spySender.Verify(s => s.SendAsync(It.IsAny<DeliverMessageEnvelope>()), Times.Once);
        }

        private IStep CreateSendDeliverStepWithSender(IDeliverSender spySender)
        {
            var stubProvider = new Mock<IDeliverSenderProvider>();
            stubProvider.Setup(p => p.GetDeliverSender(It.IsAny<string>())).Returns(spySender);

            return new SendDeliverMessageStep(stubProvider.Object, GetDataStoreContext);
        }

        private static DeliverMessageEnvelope EmptyDeliverMessageEnvelope()
        {
            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo("not-empty-message-id", "not-empty-mpc"),
                deliverMessage: new byte[] { },
                contentType: string.Empty);
        }

        [Fact]
        public async Task ThenExecuteMethodSucceedsWithValidUserMessageAsync()
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertInMessage(
                CreateInMessage(id, InStatus.Received, Operation.ToBeDelivered));

            DeliverMessageEnvelope envelope = AnonymousDeliverEnvelope(id);
            IStep sut = CreateSendDeliverStepWithSender(new SpySender());

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope) {ReceivingPMode = CreateDefaultReceivingPMode()});

            // Assert
            GetDataStoreContext.AssertInMessage(id, inmessage =>
            {
                Assert.NotNull(inmessage);
                Assert.Equal(InStatus.Delivered, inmessage.Status.ToEnum<InStatus>());
                Assert.Equal(Operation.Delivered, inmessage.Operation);
            });
        }

        [Theory]
        [ClassData(typeof(DeliverRetryData))]
        public async Task Reset_InMessage_Operation_ToBeDelivered_When_CurrentRetry_LessThen_MaxRetry(DeliverRetry input)
        {
            // Arrange
            string id = Guid.NewGuid().ToString();

            InMessage im = CreateInMessage(id, InStatus.Received, Operation.Delivering);
            GetDataStoreContext.InsertInMessage(im);

            var r = RetryReliability.CreateForInMessage(
                refToInMessageId: im.Id,
                maxRetryCount: input.MaxRetryCount,
                retryInterval: default(TimeSpan),
                type: RetryType.Notification);
            r.CurrentRetryCount = input.CurrentRetryCount;
            GetDataStoreContext.InsertRetryReliability(r);

            DeliverMessageEnvelope envelope = AnonymousDeliverEnvelope(id);

            var stub = new Mock<IDeliverSender>();
            stub.Setup(s => s.SendAsync(envelope))
                .ReturnsAsync(input.SendResult);

            IStep sut = CreateSendDeliverStepWithSender(stub.Object);

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope) { ReceivingPMode = CreateDefaultReceivingPMode() });

            // Assert
            GetDataStoreContext.AssertInMessage(id, inMessage =>
            {
                Assert.NotNull(inMessage);
                Assert.Equal(input.ExpectedStatus, inMessage.Status.ToEnum<InStatus>());
                Assert.Equal(input.ExpectedOperation, inMessage.Operation);
            });
        }

        private static InMessage CreateInMessage(string id, InStatus status, Operation operation)
        {
            var m = new InMessage(id);
            m.SetStatus(status);
            m.Operation= operation;

            return m;
        }

        private static DeliverMessageEnvelope AnonymousDeliverEnvelope(string id)
        {
            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo { MessageId = id },
                deliverMessage: new byte[] { },
                contentType: string.Empty);
        }

        private static ReceivingProcessingMode CreateDefaultReceivingPMode()
        {
            return new ReceivingProcessingMode
            {
                MessageHandling =
                {
                    DeliverInformation =
                    {
                        DeliverMethod = new Method
                        {
                            Type = "FILE"
                        }
                    }
                }
            };
        }
    }
}