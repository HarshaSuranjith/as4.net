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
                OutMessage outMessage = BuildForUserMessage(as4Message);

                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                Assert.Equal(MessageType.UserMessage, MessageTypeUtils.Parse(outMessage.EbmsMessageType));
                Assert.Equal(await AS4XmlSerializer.ToStringAsync(ExpectedPMode()), outMessage.PMode);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateAS4MessageWithUserMessage(messageId);


                // Act
                OutMessage outMessage = BuildForUserMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
            }

            private OutMessage BuildForUserMessage(AS4Message as4Message)
            {
                return OutMessageBuilder.ForMessageUnit(as4Message.FirstUserMessage, as4Message.ContentType, ExpectedPMode())
                                        .Build();
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForReceiptMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Receipt(messageId), ExpectedPMode());

                // Act
                OutMessage outMessage = BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Receipt, MessageTypeUtils.Parse(outMessage.EbmsMessageType));
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForErrorMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Error(messageId), ExpectedPMode());

                // Act
                OutMessage outMessage = BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Error, MessageTypeUtils.Parse(outMessage.EbmsMessageType));
            }

            private OutMessage BuildForSignalMessage(AS4Message as4Message)
            {
                return OutMessageBuilder.ForMessageUnit(as4Message.FirstSignalMessage, as4Message.ContentType, ExpectedPMode())
                                        .Build();
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