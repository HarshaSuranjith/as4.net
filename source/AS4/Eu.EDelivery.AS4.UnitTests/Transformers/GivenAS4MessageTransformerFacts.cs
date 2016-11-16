﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Utilities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing the <see cref="AS4MessageTransformer" />
    /// </summary>
    public class GivenAS4MessageTransformerFacts
    {
        private readonly AS4MessageTransformer _transformer;
        private AS4Message _as4Message;
        private Attachment _attachment;

        public GivenAS4MessageTransformerFacts()
        {
            this._transformer = new AS4MessageTransformer(Registry.Instance.SerializerProvider);

            CreateAS4Message();
            CreateAttachment();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        private void CreateAS4Message()
        {
            var userMessage = new UserMessage(messageId: "message-id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };

            this._as4Message = new AS4MessageBuilder()
                .WithSendingPMode(new SendingProcessingMode())
                .WithUserMessage(userMessage)
                .Build();

            this._as4Message.ContentType = Constants.ContentTypes.Soap;
        }

        private void CreateAttachment()
        {
            this._attachment = new Attachment(id: "attachment-id")
            {
                Content = new MemoryStream(),
                ContentType = "image/jpeg"
            };

            var xmlSerializer = new XmlSerializer(typeof(string));
            xmlSerializer.Serialize(this._attachment.Content, "<Root></Root>");
            this._attachment.Content.Position = 0;
        }

        protected MemoryStream WriteAS4MessageToStream(AS4Message as4Message)
        {
            var memoryStream = new MemoryStream();

            ISerializerProvider provider = new SerializerProvider();
            ISerializer serializer = provider.Get(as4Message.ContentType);
            serializer.Serialize(as4Message, memoryStream, CancellationToken.None);

            memoryStream.Position = 0;
            return memoryStream;
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
                MemoryStream memoryStream = base.WriteAS4MessageToStream(base._as4Message);
                var receivedMessage = new ReceivedMessage(memoryStream, Constants.ContentTypes.Soap);
                // Act
                InternalMessage internalMessage = await base._transformer
                    .TransformAsync(receivedMessage, CancellationToken.None);
                // Assert
                Assert.NotNull(internalMessage);
                Assert.NotNull(internalMessage.AS4Message);
            }
        }

        /// <summary>
        /// Testing if the Transformer fails
        /// for the "Transform" Method
        /// </summary>
        public class GivenInvalidArgumentsToTransfrormer : GivenAS4MessageTransformerFacts
        {
            [Fact]
            public async Task ThenTransformFailsWithInvalidUserMessageWithSoapAS4StreamAsync()
            {
                // Arrange
                base._as4Message.UserMessages = new[] {new UserMessage("message-id")};
                MemoryStream memoryStream = base.WriteAS4MessageToStream(base._as4Message);
                var receivedMessage = new ReceivedMessage(memoryStream, Constants.ContentTypes.Soap);
                // Act / Assert
                await Assert.ThrowsAsync<AutoMapperMappingException>(
                    () => base._transformer
                        .TransformAsync(receivedMessage, CancellationToken.None));
            }
        }
    }
}