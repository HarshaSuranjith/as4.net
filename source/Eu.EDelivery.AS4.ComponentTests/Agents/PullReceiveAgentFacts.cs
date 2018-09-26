﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullReceiveAgentFacts : ComponentTestTemplate
    {
        private const string PullRequestMpc =
            "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC/UK:UK001";

        private readonly Settings _pullReceiveSettings;
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullReceiveAgentFacts"/> class.
        /// </summary>
        public PullReceiveAgentFacts()
        {
            _pullReceiveSettings = OverrideSettings("pullreceiveagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

        [Fact]
        public void NoExceptionsAreLoggedWhenPullSenderIsNotAvailable()
        {
            // Arrange
            string pullSenderUrl = RetrievePullingUrlFromConfig();
            _databaseSpy.ClearDatabase();

            // Act
            var waiter = new ManualResetEvent(false);
            StubHttpServer.StartServer(pullSenderUrl, _ => throw new InvalidOperationException(), waiter);
            waiter.WaitOne(timeout: TimeSpan.FromSeconds(5));

            // Assert
            _as4Msh.Dispose();
            Assert.Empty(_databaseSpy.GetInExceptions(r => true));
        }

        [Fact]
        public async Task PiggyBack_PullRequest_With_Receipt_Operation_Becomes_Sent_When_Received_Success_HTTP_StatusCode()
        {
            // Arrange
            string pullSenderUrl = RetrievePullingUrlFromConfig();

            var user = new UserMessage($"user-{Guid.NewGuid()}", PullRequestMpc);
            var receipt = new Receipt($"receipt-{Guid.NewGuid()}", user.MessageId);

            InsertUserMessage(user);
            InsertReceipt(receipt, pullSenderUrl, Operation.ToBePiggyBacked);

            // Act
            AS4Message piggyBacked = await RespondToPullRequest(pullSenderUrl, responseStatusCode: 202);

            // Assert
            Assert.Collection(
                piggyBacked.MessageUnits,
                x => Assert.IsType<PullRequest>(x),
                x => Assert.IsType<Receipt>(x));

            OutMessage storedReceipt = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == receipt.MessageId),
                timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(Operation.Sending, storedReceipt.Operation);
        }

        [Fact]
        public async Task PiggyBack_PullRequest_With_Receipt_Operation_Resets_To_ToBePiggyBacked_When_Received_Failure_HTTP_StatusCode()
        {
            // Arrange
            string pullSenderUrl = RetrievePullingUrlFromConfig();

            var user = new UserMessage($"user-{Guid.NewGuid()}", PullRequestMpc);
            var receipt = new Receipt($"receipt-{Guid.NewGuid()}", user.MessageId);

            InsertUserMessage(user);
            InsertReceipt(receipt, pullSenderUrl, Operation.ToBePiggyBacked);

            // Act
            await RespondToPullRequest(pullSenderUrl, responseStatusCode: 500);

            // Assert
            await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(
                    m => m.EbmsMessageId == receipt.MessageId 
                         && m.Operation == Operation.ToBePiggyBacked),
                timeout: TimeSpan.FromSeconds(10));
        }

        private void InsertUserMessage(UserMessage user)
        {
            _databaseSpy.InsertInMessage(
                new InMessage(user.MessageId)
                {
                    Mpc = user.Mpc,
                    EbmsMessageType = MessageType.UserMessage,
                    ContentType = Constants.ContentTypes.Soap
                });
        }

        private void InsertReceipt(Receipt receipt, string url, Operation operation)
        {
            string location = 
                Registry.Instance
                        .MessageBodyStore
                        .SaveAS4Message(
                            _as4Msh.GetConfiguration().OutMessageStoreLocation, 
                            AS4Message.Create(receipt));

            _databaseSpy.InsertOutMessage(
                new OutMessage(receipt.MessageId)
                {
                    EbmsRefToMessageId = receipt.RefToMessageId,
                    EbmsMessageType = MessageType.Receipt,
                    ContentType = Constants.ContentTypes.Soap,
                    MessageLocation = location,
                    Url = url,
                    Operation = operation
                });
        }

        [Fact]
        public async Task Received_Bundled_Response_Should_Process_All_Messages()
        {
            // Arrange
            string pullSenderUrl = RetrievePullingUrlFromConfig();

            string storedMessageId = "stored-" + Guid.NewGuid();
            StoreToBeAckOutMessage(storedMessageId);
            AS4Message bundled = CreateBundledMultipleUserMessagesWithRefTo();
            bundled.AddMessageUnit(new Receipt(storedMessageId));

            // Act
            await RespondToPullRequest(
                pullSenderUrl,
                response =>
                {
                    response.ContentType = bundled.ContentType;
                    response.StatusCode = 200;
                    using (Stream output = response.OutputStream)
                    {
                        SerializerProvider.Default
                            .Get(bundled.ContentType)
                            .Serialize(bundled, output, CancellationToken.None);
                    }
                });

            // Assert
            IEnumerable<InMessage> storedBundled =
                await PollUntilPresent(
                    () => _databaseSpy.GetInMessages(bundled.UserMessages.Select(u => u.MessageId).ToArray())
                                      .Where(m => m.Operation == Operation.ToBeDelivered),
                    timeout: TimeSpan.FromSeconds(20));

            Assert.Collection(
                storedBundled,
                userMessage1 =>
                {
                    Assert.Equal(MessageExchangePattern.Pull, userMessage1.MEP);
                    Assert.Equal(InStatus.Received, userMessage1.Status.ToEnum<InStatus>());
                    Assert.Equal(Operation.ToBeDelivered, userMessage1.Operation);
                },
                userMessage2 =>
                {
                    Assert.Equal(MessageExchangePattern.Pull, userMessage2.MEP);
                    Assert.Equal(InStatus.Received, userMessage2.Status.ToEnum<InStatus>());
                    Assert.Equal(Operation.ToBeDelivered, userMessage2.Operation);
                });
            Assert.Collection(
                _databaseSpy.GetInMessages(bundled.SignalMessages.Select(s => s.MessageId).ToArray()),
                signal =>
                {
                    Assert.Equal(MessageExchangePattern.Pull, signal.MEP);
                    Assert.Equal(InStatus.Received, signal.Status.ToEnum<InStatus>());
                    Assert.Equal(Operation.ToBeNotified, signal.Operation);
                });
            Assert.Collection(
                _databaseSpy.GetOutMessages(storedMessageId),
                stored => Assert.Equal(OutStatus.Ack, stored.Status.ToEnum<OutStatus>()));
        }

        private void StoreToBeAckOutMessage(string storedMessageId)
        {
            var storedUserMessage = new OutMessage(ebmsMessageId: storedMessageId);
            storedUserMessage.EbmsMessageType = MessageType.UserMessage;
            storedUserMessage.SetStatus(OutStatus.Sent);

            _databaseSpy.InsertOutMessage(storedUserMessage);
        }

        private static AS4Message CreateBundledMultipleUserMessagesWithRefTo()
        {
            var userMessage1 = new UserMessage(
                messageId: "user1-" + Guid.NewGuid(),
                collaboration: new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "pullreceive_bundled_pmode"),
                    service: new Service(
                        value: "bundling",
                        type: "as4:net:pullreceive:bundling"),
                    action: "as4:net:pullreceive:bundling",
                    conversationId: "as4:net:pullreceive:conversation"));

            var userMessage2 = new UserMessage(
                messageId: "user2-" + Guid.NewGuid(),
                collaboration: new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "some-other-pmode-id"),
                    service: new Service(
                        value: "bundling",
                        type: "as4:net:pullreceive:bundling"),
                    action: "as4:net:pullreceive:bundling",
                    conversationId: "as4:net:pullreceive:conversation"));

            var bundled = AS4Message.Create(userMessage1);
            bundled.AddMessageUnit(userMessage2);

            return bundled;
        }

        private string RetrievePullingUrlFromConfig()
        {
            AgentSettings pullReceiveAgent = _pullReceiveSettings.Agents.PullReceiveAgents.FirstOrDefault();

            if (pullReceiveAgent == null)
            {
                throw new ConfigurationErrorsException("There is no PullReceive Agent configured.");
            }

            string pmodeId = pullReceiveAgent.Receiver.Setting.First().Key;

            SendingProcessingMode pmode = _as4Msh.GetConfiguration().GetSendingPMode(pmodeId);

            if (pmode == null)
            {
                throw new ConfigurationErrorsException($"No Sending PMode found with Id {pmodeId}");
            }

            return pmode.PushConfiguration.Protocol.Url;
        }

        private static async Task RespondToPullRequest(string url, Action<HttpListenerResponse> response)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var waiter = new ManualResetEvent(false);
            StubHttpServer.StartServer(url, response, waiter);
            waiter.WaitOne(timeout: TimeSpan.FromSeconds(10));

            // Wait till the response is processed correctly.
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        private async Task<AS4Message> RespondToPullRequest(string url, int responseStatusCode)
        {
            var input = new VirtualStream();
            var waiter = new ManualResetEvent(false);
            StubHttpServer.StartServer(
                url,
                (req, res) =>
                {
                    req.InputStream.CopyTo(input);
                    res.StatusCode = responseStatusCode;
                    res.OutputStream.Dispose();
                },
                waiter);

            waiter.WaitOne(timeout: TimeSpan.FromSeconds(5));

            // Wait till the response is processed correctly.
            await Task.Delay(TimeSpan.FromSeconds(2));
            _as4Msh.Dispose();

            input.Position = 0;
            var deserializeTask =
                SerializerProvider
                    .Default
                    .Get(Constants.ContentTypes.Soap)
                    .DeserializeAsync(input, Constants.ContentTypes.Soap, CancellationToken.None);

            deserializeTask.Wait(TimeSpan.FromSeconds(1));
            AS4Message result = deserializeTask.Result;

            Assert.True(result != null, "PullRequest couldn't be deserialized");
            return result;
        }

        private static AS4Message CreateUserMessageResponse()
        {
            return AS4Message.Create(
                new UserMessage(
                    $"user-{Guid.NewGuid()}", 
                    PullRequestMpc,
                    new CollaborationInfo(
                        new AgreementReference(
                            "http://eu.europe.agreements.org",
                            "pullreceive_bundled_pmode"),
                        Service.TestService,
                        Constants.Namespaces.TestAction,
                        CollaborationInfo.DefaultConversationId),
                    Model.Core.Party.DefaultFrom,
                    Model.Core.Party.DefaultTo,
                    Enumerable.Empty<PartInfo>(), 
                    Enumerable.Empty<MessageProperty>()));
        }
    }
}
