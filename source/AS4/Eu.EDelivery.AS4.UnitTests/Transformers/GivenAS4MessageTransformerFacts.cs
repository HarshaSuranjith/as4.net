﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing the <see cref="AS4MessageTransformer" />
    /// </summary>
    public class GivenAS4MessageTransformerFacts
    {
        public GivenAS4MessageTransformerFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        /// <summary>
        /// Testing if the Transformer succeeds
        /// for the "Transform" Method
        /// </summary>
        public class GivenValidReceivedMessageToTransformer : GivenAS4MessageTransformerFacts
        {
            [Fact]
            public async Task ThenTransfromSuceedsWithSoapdAS4StreamAsync()
            {
                // Arrange
                const string contentType = "multipart/related; boundary=\"=-PHQq1fuE9QxpIWax7CKj5w==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

                // Act
                MessagingContext context = await ExerciseTransform(Properties.Resources.as4_single_payload, contentType);
                
                // Assert
                Assert.NotNull(context);
                Assert.NotNull(context.AS4Message);
            }

            private async Task<MessagingContext> ExerciseTransform(byte[] contents, string contentType)
            {
                using (var stream = new MemoryStream(contents))
                {
                    var receivedMessage = new ReceivedMessage(stream, contentType);

                    return await Transform(receivedMessage);
                }
            }
        }

        /// <summary>
        /// Testing if the Transformer fails
        /// for the "Transform" Method
        /// </summary>
        public class GivenInvalidArgumentsToTransfrormer : GivenAS4MessageTransformerFacts
        {
            [Fact]
            public void FailsToCreateTransformer_IfInvalidProvider()
            {
                // Act / Assert
                Assert.ThrowsAny<Exception>(() => new AS4MessageTransformer(provider: null));
            }

            [Fact]
            public async Task ThenTransformFailsWithInvalidUserMessageWithSoapAS4StreamAsync()
            {
                // Arrange
                AS4Message as4Message = CreateAS4MessageWithoutAttachments();
                as4Message.UserMessages = new[] { new UserMessage("message-id") };
                MemoryStream memoryStream = as4Message.ToStream();

                var receivedMessage = new ReceivedMessage(memoryStream, Constants.ContentTypes.Soap);

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(() => Transform(receivedMessage));
            }

            [Fact]
            public async Task ThenTransformFails_IfContentIsNotSupported()
            {
                // Arrange
                var saboteurMessage = new ReceivedMessage(Stream.Null, "not-supported-content-type");

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(() => Transform(saboteurMessage));
            }

            [Fact]
            public async Task ThenTransformFails_IfRequestStreamIsNull()
            {
                // Arrange
                var saboteurMessage = new ReceivedMessage(requestStream: null);

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(() => Transform(saboteurMessage));
            }
        }

        private static AS4Message CreateAS4MessageWithoutAttachments()
        {
            var userMessage = new UserMessage("message-id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };

            AS4Message as4Message = AS4Message.Create(userMessage);

            as4Message.ContentType = Constants.ContentTypes.Soap;

            return as4Message;
        }

        protected async Task<MessagingContext> Transform(ReceivedMessage message)
        {
            var transformer = new AS4MessageTransformer(Registry.Instance.SerializerProvider);
            return await transformer.TransformAsync(message, CancellationToken.None);
        }
    }
}