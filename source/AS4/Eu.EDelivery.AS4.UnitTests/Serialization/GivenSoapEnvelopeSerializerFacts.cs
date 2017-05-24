﻿using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Resources;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing <see cref="SoapEnvelopeSerializer" />
    /// </summary>
    public class GivenSoapEnvelopeSerializerFacts
    {
        private readonly SoapEnvelopeSerializer _serializer;

        public GivenSoapEnvelopeSerializerFacts()
        {
            _serializer = new SoapEnvelopeSerializer();
        }

        /// <summary>
        /// Testing if the serializer succeeds
        /// </summary>
        public class GivenSoapEnvelopeSerializerSucceeds : GivenSoapEnvelopeSerializerFacts
        {
            private const string ServiceNamespace = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/service";
            private const string ActionNamespace = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/test";

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsAsync()
            {
                // Arrange
                using (MemoryStream memoryStream = CreateAnonymousAS4Message().ToStream())
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    Assert.Equal(1, message.UserMessages.Count);
                }
            }

            [Fact]
            public async void ThenParseUserMessageCollaborationInfoCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal(ServiceNamespace, userMessage.CollaborationInfo.Service.Value);
                    Assert.Equal(ActionNamespace, userMessage.CollaborationInfo.Action);
                    Assert.Equal("eu:edelivery:as4:sampleconversation", userMessage.CollaborationInfo.ConversationId);
                }
            }

            [Fact]
            public async Task ThenParseUserMessagePropertiesParsedCorrectlyAsync()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.NotNull(message);
                    Assert.Equal(1, message.UserMessages.Count);
                    Assert.Equal(1472800326948, userMessage.Timestamp.ToUnixTimeMilliseconds());
                }
            }

            [Fact]
            public async void ThenParseUserMessageReceiverCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF32.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    string receiverId = userMessage.Receiver.PartyIds.First().Id;
                    Assert.Equal("org:holodeckb2b:example:company:B", receiverId);
                    Assert.Equal("Receiver", userMessage.Receiver.Role);
                }
            }

            [Fact]
            public async void ThenParseUserMessageSenderCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal("org:eu:europa:as4:example", userMessage.Sender.PartyIds.First().Id);
                    Assert.Equal("Sender", userMessage.Sender.Role);
                }
            }

            [Fact]
            public void ThenXmlDocumentContainsOneMessagingHeader()
            {
                // Arrange
                using (var memoryStream = new MemoryStream())
                {
                    AS4Message dummyMessage = CreateAnonymousAS4Message();

                    // Act
                    _serializer.Serialize(dummyMessage, stream: memoryStream, cancellationToken: CancellationToken.None);

                    // Assert
                    AssertXmlDocumentContainsMessagingTag(memoryStream);
                }
            }

            private static AS4Message CreateAnonymousAS4Message()
            {
                return new AS4MessageBuilder()
                    .WithUserMessage(CreateAnonymousUserMessage())
                    .Build();
            }

            private static UserMessage CreateAnonymousUserMessage()
            {
                return new UserMessage("message-Id")
                {
                    Receiver = new Party("Receiver", new PartyId()),
                    Sender = new Party("Sender", new PartyId())
                };
            }

            private static void AssertXmlDocumentContainsMessagingTag(Stream stream)
            {
                stream.Position = 0;
                using (var reader = new XmlTextReader(stream))
                {
                    var document = new XmlDocument();
                    document.Load(reader);
                    XmlNodeList nodeList = document.GetElementsByTagName("eb:Messaging");
                    Assert.Equal(1, nodeList.Count);
                }
            }
        }

        public class GivenMultiHopSoapEnvelopeSerializerSucceeds
        {
            [Fact]
            public async Task DeserializeMultihopSignalMessage()
            {
                // Arrange
                const string contentType = "multipart/related; boundary=\"=-M/sMGEhQK8RBNg/21Nf7Ig==\";\ttype=\"application/soap+xml\"";
                string messageString = Encoding.UTF8.GetString(as4_multihop_message).Replace((char)0x1F, ' ');
                byte[] messageContent = Encoding.UTF8.GetBytes(messageString);
                using (var messageStream = new MemoryStream(messageContent))
                {
                    var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

                    // Act
                    AS4Message actualMessage = await serializer.DeserializeAsync(messageStream, contentType, CancellationToken.None);

                    // Assert
                    Assert.True(actualMessage.IsSignalMessage);
                }
            }

            [Fact]
            public void MultihopUserMessageCreatedWhenSpecifiedInPMode()
            {
                // Arrange
                AS4Message as4Message = CreateAS4Message();
                as4Message.SendingPMode = CreateMultihopPMode();
                // Act
                XmlDocument doc = AS4XmlSerializer.ToDocument(as4Message, CancellationToken.None);

                // Assert
                var messagingNode = doc.SelectSingleNode("//*[local-name()='Messaging']") as XmlElement;

                Assert.NotNull(messagingNode);
                Assert.Equal(Constants.Namespaces.EbmsNextMsh, messagingNode.GetAttribute("role", Constants.Namespaces.Soap12));
                Assert.True(XmlConvert.ToBoolean(messagingNode.GetAttribute("mustUnderstand", Constants.Namespaces.Soap12)));
            }

            [Fact]
            public async void ReceiptMessageForMultihopUserMessageIsMultihop()
            {
                AS4Message as4Message = await CreateReceivedAS4Message();

                var message = new InternalMessage
                {
                    AS4Message = as4Message
                };

                // Create a receipt for this message.
                // Use the CreateReceiptStep, since there is no other way.
                var step = new CreateAS4ReceiptStep();
                StepResult result = await step.ExecuteAsync(message, CancellationToken.None);

                // The result should contain a signalmessage, which is a receipt.
                Assert.True(result.InternalMessage.AS4Message.IsSignalMessage);

                XmlDocument doc = AS4XmlSerializer.ToDocument(result.InternalMessage.AS4Message, CancellationToken.None);

                // Following elements should be present:
                // - To element in the wsa namespace
                // - Action element in the wsa namespace
                // - UserElement in the multihop namespace.
                AssertToElement(doc);
                AssertActionElement(doc);
                AssertUserMessageElement(doc);
                AssertUserMessageMessagingElement(as4Message, doc);

                AssertIfSenderAndReceiverAreReversed(as4Message, doc);
            }

            [Fact]
            public async Task CanDeserializeAndReSerializeMultihopReceipt()
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(multihopreceipt)))
                {
                    var multihopReceipt = await SerializerProvider.Default.Get(Constants.ContentTypes.Soap).DeserializeAsync(stream, Constants.ContentTypes.Soap, CancellationToken.None);

                    Assert.NotNull(multihopReceipt);
                    Assert.NotNull(multihopReceipt.PrimarySignalMessage);
                    Assert.NotNull(multihopReceipt.PrimarySignalMessage.MultiHopRouting);

                    // Serialize the Deserialized receipt again, and make sure the RoutingInput element is present and correct.
                    XmlDocument doc = AS4XmlSerializer.ToDocument(multihopReceipt, CancellationToken.None);

                    var routingInput = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");

                    Assert.NotNull(routingInput);
                }
            }

            [Fact]
            public async Task ErrorMessageForMultihopUserMessageIsMultihop()
            {
                // Arrange
                AS4Message expectedAS4Message = await CreateReceivedAS4Message();

                Error error = new ErrorBuilder()
                    .WithOriginalAS4Message(expectedAS4Message)
                    .WithRefToEbmsMessageId(expectedAS4Message.PrimaryUserMessage.MessageId)
                    .Build();

                AS4Message errorMessage = new AS4MessageBuilder()
                    .WithSignalMessage(error)
                    .WithSendingPMode(CreateMultihopPMode())
                    .Build();

                // Act
                XmlDocument document = AS4XmlSerializer.ToDocument(errorMessage, CancellationToken.None);

                // Following elements should be present:
                // - To element in the wsa namespace
                // - Action element in the wsa namespace
                // - UserElement in the multihop namespace.
                AssertToElement(document);
                AssertActionElement(document);
                AssertUserMessageElement(document);

                AssertMessagingElement(document);
                AssertIfSenderAndReceiverAreReversed(expectedAS4Message, document);
            }

            private static void AssertUserMessageMessagingElement(AS4Message as4Message, XmlNode doc)
            {
                AssertMessagingElement(doc);

                string actualRefToMessageId = DeserializeMessagingHeader(doc).SignalMessage.First().MessageInfo.RefToMessageId;
                string expectedUserMessageId = as4Message.PrimaryUserMessage.MessageId;

                Assert.Equal(expectedUserMessageId, actualRefToMessageId);
            }

            private static void AssertToElement(XmlNode doc)
            {
                XmlNode toAddressing = doc.SelectSingleNode($@"//*[local-name()='To' and namespace-uri()='{Constants.Namespaces.Addressing}']");

                Assert.NotNull(toAddressing);
                Assert.Equal(Constants.Namespaces.ICloud, toAddressing.InnerText);
            }

            private static void AssertUserMessageElement(XmlNode doc)
            {
                Assert.NotNull(doc.SelectSingleNode($@"//*[local-name()='UserMessage' and namespace-uri()='{Constants.Namespaces.EbmsMultiHop}']"));
            }

            private static void AssertActionElement(XmlNode doc)
            {
                Assert.NotNull(doc.SelectSingleNode($@"//*[local-name()='Action' and namespace-uri()='{Constants.Namespaces.Addressing}']"));
            }

            private static void AssertMessagingElement(XmlNode doc)
            {
                Messaging messaging = DeserializeMessagingHeader(doc);
                Assert.True(messaging.mustUnderstand1);
                Assert.Equal(Constants.Namespaces.EbmsNextMsh, messaging.role);
            }

            private static Messaging DeserializeMessagingHeader(XmlNode doc)
            {
                XmlNode messagingNode = doc.SelectSingleNode(@"//*[local-name()='Messaging']");
                Assert.NotNull(messagingNode);

                return AS4XmlSerializer.FromString<Messaging>(messagingNode.OuterXml);
            }

            private static void AssertIfSenderAndReceiverAreReversed(AS4Message expectedAS4Message, XmlNode doc)
            {
                XmlNode routingInputNode = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");
                Assert.NotNull(routingInputNode);
                var routingInput = AS4XmlSerializer.FromString<RoutingInput>(routingInputNode.OuterXml);

                RoutingInputUserMessage actualUserMessage = routingInput.UserMessage;
                UserMessage expectedUserMessage = expectedAS4Message.PrimaryUserMessage;

                Assert.Equal(expectedUserMessage.Sender.Role, actualUserMessage.PartyInfo.To.Role);
                Assert.Equal(expectedUserMessage.Sender.PartyIds.First().Id, actualUserMessage.PartyInfo.To.PartyId.First().Value);
                Assert.Equal(expectedUserMessage.Receiver.Role, actualUserMessage.PartyInfo.From.Role);
                Assert.Equal(expectedUserMessage.Receiver.PartyIds.First().Id, actualUserMessage.PartyInfo.From.PartyId.First().Value);
            }

            private static AS4Message CreateAS4Message()
            {
                var sender = new Party("sender", new PartyId("senderId"));
                var receiver = new Party("rcv", new PartyId("receiverId"));

                return new AS4MessageBuilder()
                    .WithUserMessage(new UserMessage { Sender = sender, Receiver = receiver })
                    .Build();
            }

            private static SendingProcessingMode CreateMultihopPMode()
            {
                return new SendingProcessingMode
                {
                    Id = "multihop-pmode",
                    MessagePackaging = { IsMultiHop = true }
                };
            }

            private static async Task<AS4Message> CreateReceivedAS4Message()
            {
                var message = CreateAS4Message();
                message.SendingPMode = CreateMultihopPMode();

                var serializer = SerializerProvider.Default.Get(message.ContentType);

                // Serialize and deserialize the AS4 Message to simulate a received message.
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(message, stream, CancellationToken.None);
                    stream.Position = 0;
                    return await serializer.DeserializeAsync(stream, message.ContentType, CancellationToken.None);
                }
            }
        }

        public class GivenReceiptSerializationSucceeds : GivenSoapEnvelopeSerializerFacts
        {
            [Fact]
            public void ThenNonRepudiationInfoElementBelongsToCorrectNamespace()
            {
                var receipt = CreateReceiptWithNonRepudiationInfo();

                var as4Message = new AS4MessageBuilder().WithSignalMessage(receipt).Build();

                XmlDocument document = AS4XmlSerializer.ToDocument(as4Message, CancellationToken.None);

                var node = document.SelectSingleNode(@"//*[local-name()='NonRepudiationInformation']");

                Assert.NotNull(node);
                Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
            }

            [Fact]
            public void ThenRelatedUserMessageElementBelongsToCorrectNamespace()
            {
                var receipt = CreateReceiptWithRelatedUserMessageInfo();

                var as4Message = new AS4MessageBuilder().WithSignalMessage(receipt).Build();

                XmlDocument document = AS4XmlSerializer.ToDocument(as4Message, CancellationToken.None);

                var node = document.SelectSingleNode(@"//*[local-name()='UserMessage']");

                Assert.NotNull(node);
                Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
            }

            private static Receipt CreateReceiptWithNonRepudiationInfo()
            {
                var nnri = new ArrayList { new System.Security.Cryptography.Xml.Reference() };

                var receipt = new Receipt
                {
                    NonRepudiationInformation = new NonRepudiationInformationBuilder().WithSignedReferences(nnri).Build()
                };

                return receipt;
            }

            private static Receipt CreateReceiptWithRelatedUserMessageInfo()
            {
                var receipt = new Receipt
                {
                    UserMessage = new UserMessage("some-usermessage-id")
                };

                return receipt;
            }
        }
    }
}