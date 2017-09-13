using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="ExceptionToNotifyMessageTransformer"/>
    /// </summary>
    public class GivenExceptionToNotifyMessageTransformerFacts
    {
        [Fact]
        public async void ThenInExceptionIsTransformedToNotifyEnvelope()
        {
            // Arrange
            ReceivedEntityMessage receivedMessage = CreateReceivedExceptionMessage(new InException("id", "refid"), Operation.ToBeNotified);
            var transformer = new ExceptionToNotifyMessageTransformer();
            var result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(
                ((ExceptionEntity)receivedMessage.Entity).EbmsRefToMessageId,
                result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenOutExceptionIsTransformedToNotifyEnvelope()
        {
            // Arrange
            ReceivedEntityMessage receivedMessage = CreateReceivedExceptionMessage(new OutException("id", "refid"), Operation.ToBeNotified);
            var transformer = new ExceptionToNotifyMessageTransformer();

            // Act
            MessagingContext result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(
                ((ExceptionEntity)receivedMessage.Entity).EbmsRefToMessageId,
                result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenTransformSucceedsWithValidInExceptionForErrorPropertiesAsync()
        {
            // Arrange            
            ReceivedEntityMessage receivedMessage = CreateReceivedExceptionMessage(new InException("id", "refid"), Operation.ToBeNotified);
            var transformer = new ExceptionToNotifyMessageTransformer();

            // Act
            MessagingContext messagingContext =
                await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.Equal(Status.Exception, messagingContext.NotifyMessage.StatusCode);
            Assert.Equal(((InException)receivedMessage.Entity).EbmsRefToMessageId, messagingContext.NotifyMessage.MessageInfo.RefToMessageId);
        }

        private static ReceivedEntityMessage CreateReceivedExceptionMessage(ExceptionEntity exceptionEntity, Operation exceptionOperation)
        {            
            exceptionEntity.SetOperation(exceptionOperation);

            return new ReceivedEntityMessage(exceptionEntity);
        }

        [Fact]
        public async Task FaisToTransform_IfNotSupported()
        {
            // Arrange
            var sut = new ExceptionToNotifyMessageTransformer();

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(new ReceivedMessage(Stream.Null), CancellationToken.None));

            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(new ReceivedEntityMessage(new InMessage(Guid.NewGuid().ToString())), CancellationToken.None));
        }
    }
}