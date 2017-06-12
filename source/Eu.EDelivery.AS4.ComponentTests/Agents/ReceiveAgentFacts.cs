﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _receiveAgentUrl;
        private readonly DatabaseSpy _dbSpy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveAgentFacts"/> class.
        /// </summary>
        public ReceiveAgentFacts()
        {
            OverrideSettings("receiveagent_http_settings.xml");

            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            _dbSpy = new DatabaseSpy(_as4Msh.GetConfiguration());

            var receivingAgent = _as4Msh.GetConfiguration().GetSettingsAgents().FirstOrDefault(a => a.Name.Equals("Receive Agent"));

            if (receivingAgent == null)
            {
                throw new ConfigurationErrorsException("The Agent with name Receive Agent could not be found");
            }

            _receiveAgentUrl = receivingAgent.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;

            if (String.IsNullOrWhiteSpace(_receiveAgentUrl))
            {
                throw new ConfigurationErrorsException("The URL where the receive agent is listening on, could not be retrieved.");
            }
        }

        public class GivenValidReceivedUserMessageFacts : ReceiveAgentFacts
        {
            [Fact]
            public async Task ThenInMessageOperationIsToBeDelivered()
            {
                var sendMessage = CreateSendAS4Message();

                var response = await _httpClient.SendAsync(sendMessage);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var receivedAS4Message =
                    await SerializerProvider.Default.Get(response.Content.Headers.ContentType.MediaType)
                                            .DeserializeAsync(await response.Content.ReadAsStreamAsync(),
                                                              response.Content.Headers.ContentType.MediaType, CancellationToken.None);
                Assert.True(receivedAS4Message.IsSignalMessage);
                Assert.True(receivedAS4Message.PrimarySignalMessage is Receipt);

                // Check if the status of the received UserMessage is set to 'ToBeDelivered'

                var receivedUserMessage = _dbSpy.GetInMessageFor(i => i.EbmsMessageId.Equals(receivedAS4Message.PrimarySignalMessage.RefToMessageId));
                Assert.NotNull(receivedUserMessage);
                Assert.Equal(Operation.ToBeDelivered, receivedUserMessage.Operation);
            }

            private HttpRequestMessage CreateSendAS4Message()
            {
                var message = new HttpRequestMessage(HttpMethod.Post, _receiveAgentUrl);

                message.Content = new ByteArrayContent(Properties.Resources.receiveagent_message);
                message.Content.Headers.Add("Content-Type", "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");

                return message;
            }

            [Fact]
            public async Task ReturnsEmptyMessageFromInvalidMessage_IfReceivePModeIsCallback()
            {
                // Arrange
                HttpRequestMessage request = WrongEncryptedAS4Message();

                // Act
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // Assert
                Assert.Empty(await response.Content.ReadAsStringAsync());
            }

            private HttpRequestMessage WrongEncryptedAS4Message()
            {
                var messageContent = new ByteArrayContent(Properties.Resources.receiveagent_wrong_encrypted_message)
                {
                    Headers =
                    {
                        {
                            "Content-Type",
                            "multipart/related; boundary=\"=-WoWSZIFF06iwFV8PHCZ0dg==\"; type=\"application/soap+xml\"; charset=\"utf-8\""
                        }
                    }
                };

                return new HttpRequestMessage(HttpMethod.Post, _receiveAgentUrl) { Content = messageContent };
            }
        }

        public class GivenValidReceivedSignalMessageFacts : ReceiveAgentFacts
        {
            [Fact]
            public async Task ThenRelatedUserMessageIsAcked()
            {
                // Arrange
                CreateExistingOutMessage("message-id");

                // Act
                var as4Message = CreateAS4ReceiptMessage("message-id");

                var messageToSend = CreateSendMessage(as4Message);

                await _httpClient.SendAsync(messageToSend);

                // Assert
                // Check if the Status of the OutMessage is set to Ack
                var outMessage = _dbSpy.GetOutMessageFor(m => m.EbmsMessageId == "message-id");
                Assert.NotNull(outMessage);
                Assert.Equal(OutStatus.Ack, outMessage.Status);

                // Check if there exist an InMessage for the receipt
                var inMessage = _dbSpy.GetInMessageFor(m => m.EbmsRefToMessageId == "message-id");
                Assert.NotNull(inMessage);
                Assert.Equal(InStatus.Received, inMessage.Status);
                Assert.Equal(Operation.ToBeNotified, inMessage.Operation);
            }

            [Fact]
            public async Task ThenRelatedUserMessageIsNotAcked()
            {
                // Arrange
                CreateExistingOutMessage("message-id");

                // Act
                var as4Message = CreateAS4ErrorMessage("message-id");

                var messageToSend = CreateSendMessage(as4Message);

                await _httpClient.SendAsync(messageToSend);

                // Assert
                // Check if the Status of the OutMessage is set to Ack
                var outMessage = _dbSpy.GetOutMessageFor(m => m.EbmsMessageId == "message-id");
                Assert.NotNull(outMessage);
                Assert.Equal(OutStatus.Nack, outMessage.Status);

                // Check if there exist an InMessage for the receipt
                var inMessage = _dbSpy.GetInMessageFor(m => m.EbmsRefToMessageId == "message-id");
                Assert.NotNull(inMessage);
                Assert.Equal(InStatus.Received, inMessage.Status);
                Assert.Equal(Operation.ToBeNotified, inMessage.Operation);
            }

            private HttpRequestMessage CreateSendMessage(AS4Message message)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _receiveAgentUrl);

                byte[] serializedMessage;

                using (var stream = new MemoryStream())
                {
                    var serializer = SerializerProvider.Default.Get(message.ContentType);
                    serializer.Serialize(message, stream, CancellationToken.None);

                    serializedMessage = stream.ToArray();
                }

                requestMessage.Content = new ByteArrayContent(serializedMessage);
                requestMessage.Content.Headers.Add("Content-Type", message.ContentType);

                return requestMessage;
            }

            private static AS4Message CreateAS4ReceiptMessage(string refToMessageId)
            {
                var r = new Receipt { RefToMessageId = refToMessageId };

                return new AS4MessageBuilder().WithSignalMessage(r).Build();
            }

            private static AS4Message CreateAS4ErrorMessage(string refToMessageId)
            {
                var exception = AS4ExceptionBuilder.WithDescription("An error occurred").WithMessageIds(refToMessageId).WithErrorCode(ErrorCode.Ebms0010).Build();

                var e = new ErrorBuilder().WithRefToEbmsMessageId(refToMessageId)
                                  .WithAS4Exception(exception)
                                  .Build();

                return new AS4MessageBuilder().WithSignalMessage(e).Build();
            }

            private void CreateExistingOutMessage(string messageId)
            {
                var outMessage = new OutMessage();
                outMessage.EbmsMessageId = messageId;
                outMessage.Status = OutStatus.Sent;
                outMessage.PMode = AS4XmlSerializer.ToString(GetSendingPMode());

                _dbSpy.InsertOutMessage(outMessage);
            }

            private static SendingProcessingMode GetSendingPMode()
            {
                var pmode = new SendingProcessingMode();

                pmode.Id = "receive_agent_facts_pmode";

                pmode.ReceiptHandling.NotifyMessageProducer = true;

                pmode.ErrorHandling.NotifyMessageProducer = true;

                return pmode;
            }

        }

        // TODO:
        //     Create a test that verifies : 
        //     - Exception when the receipt is invalid (also, an InException should be created)

        // - Create a test that verifies if the Status for a received UserMessage is set to
        //      - Exception when the UserMessage is not valid (an InException should be present).

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
