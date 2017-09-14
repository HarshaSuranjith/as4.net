﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="OutMessageBuilder" />
    /// </summary>
    public class GivenOutMessageBuilderFacts
    {
        public class GivenValidArguments : GivenOutMessageBuilderFacts
        {
            [Fact]
            public async Task ThenBuildOutMessageSucceedsWithAS4Message()
            {
                // Arrange
                AS4Message as4Message = CreateAS4MessageWithUserMessage(Guid.NewGuid().ToString());

                // Act
                OutMessage outMessage = await BuildForUserMessage(as4Message);

                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                Assert.Equal(MessageType.UserMessage, MessageTypeUtils.Parse(outMessage.EbmsMessageType));
                Assert.Equal(await AS4XmlSerializer.ToStringAsync(ExpectedPMode()), outMessage.PMode);
            }

            [Fact]
            public async Task ThenBuildOutMessageSucceedsWithAS4MessageAndEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateAS4MessageWithUserMessage(messageId);


                // Act
                OutMessage outMessage = await BuildForUserMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
            }

            private async Task<OutMessage> BuildForUserMessage(AS4Message as4Message)
            {
                return await OutMessageBuilder.ForMessageUnit(as4Message.PrimaryUserMessage, as4Message.ContentType, ExpectedPMode())
                                              .BuildAsync(CancellationToken.None);
            }

            [Fact]
            public async Task ThenBuildOutMessageSucceedsForReceiptMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Receipt(messageId), ExpectedPMode());

                // Act
                OutMessage outMessage = await BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Receipt, MessageTypeUtils.Parse(outMessage.EbmsMessageType));
            }

            [Fact]
            public async Task ThenBuildOutMessageSucceedsForErrorMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Error(messageId), ExpectedPMode());

                // Act
                OutMessage outMessage = await BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Error, MessageTypeUtils.Parse(outMessage.EbmsMessageType));
            }

            private async Task<OutMessage> BuildForSignalMessage(AS4Message as4Message)
            {
                return await OutMessageBuilder.ForMessageUnit(as4Message.PrimarySignalMessage, as4Message.ContentType, ExpectedPMode())
                                              .BuildAsync(CancellationToken.None);
            }
        }

        protected SendingProcessingMode ExpectedPMode()
        {
            return new SendingProcessingMode { Id = "pmode-id" };
        }

        protected AS4Message CreateAS4MessageWithUserMessage(string messageId)
        {
            return AS4Message.Create(new UserMessage(messageId), ExpectedPMode());
        }
    }
}