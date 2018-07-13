﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class DeliverAgentFacts : ComponentTestTemplate
    {
        private const string ContentType =
            "multipart/related; boundary=\"MIMEBoundary_18bd76d83b2fa5adb6f4e198ff24bcc40fcdb2988035bd08\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private static readonly string DeliveryRoot = Path.Combine(Environment.CurrentDirectory, @"messages\in");

        private readonly AS4Component _as4Msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverAgentFacts"/> class.
        /// </summary>
        public DeliverAgentFacts()
        {
            OverrideSettings("deliveragent_http_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task Deliver_Attachment_Only_If_Belong_To_UserMessage()
        {
            // Arrange
            AS4Message as4Message = await CreateAS4MessageFrom(deliveragent_message);
            as4Message.AddAttachment(StubAttachment());

            FileSystemUtils.ClearDirectory(DeliveryRoot);

            // Act
            await InsertToBeDeliveredMessage(as4Message);

            // Assert
            AssertOnDeliveredAttachments(DeliveryRoot, files => Assert.True(files.Length == 1, "files.Length == 1"));
        }

        private static Attachment StubAttachment()
        {
            string uri = Path.Combine(Environment.CurrentDirectory, "messages", "attachments", "earth.jpg");
            var stream = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read);

            return new Attachment(
                id: "yet-another-attachment",
                content: stream,
                location: uri,
                contentType: "image/jpeg");
        }

        private static void AssertOnDeliveredAttachments(string location, Action<FileInfo[]> assertion)
        {
            // Wait till the AS4 Component has updated the record
            Thread.Sleep(TimeSpan.FromSeconds(6));

            FileInfo[] files = new DirectoryInfo(location).GetFiles("*.jpg");
            assertion(files);
        }

        [Fact]
        public async Task Deliver_Message_Only_When_Referenced_Payloads_Are_Delivered()
        {
            AS4Message as4Message = await CreateAS4MessageFrom(deliveragent_message);

            string deliverLocation = DeliverPayloadLocationOf(as4Message.Attachments.First());
            CleanDirectoryAt(Path.GetDirectoryName(deliverLocation));

            // Act
            IPMode pmode = CreateReceivedPMode(
                deliverMessageLocation: DeliveryRoot,
                deliverPayloadLocation: @"%# \ (+_O) / -> Not a valid path");

            InMessage inMessage = CreateInMessageRepresentingUserMessage(as4Message, pmode);
            await InsertInMessage(inMessage);

            // Assert
            var spy = new DatabaseSpy(_as4Msh.GetConfiguration());
            InMessage actual = await PollUntilPresent(
                () => spy.GetInMessageFor(im => im.Id == inMessage.Id && im.Status == InStatus.Exception.ToString()),
                TimeSpan.FromSeconds(10));

            Assert.Empty(Directory.EnumerateFiles(DeliveryRoot));
            Assert.Equal(InStatus.Exception, actual.Status.ToEnum<InStatus>());
            Assert.Equal(Operation.DeadLettered, actual.Operation);
        }


        private static async Task<AS4Message> CreateAS4MessageFrom(byte[] deliveragentMessage)
        {
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
            return await serializer.DeserializeAsync(
                       new MemoryStream(deliveragentMessage),
                       ContentType,
                       CancellationToken.None);
        }

        private static void CleanDirectoryAt(string location)
        {
            foreach (FileInfo file in new DirectoryInfo(location).EnumerateFiles())
            {
                file.Delete();
            }
        }

        private static string DeliverPayloadLocationOf(Attachment a)
        {
            return Path.Combine(Environment.CurrentDirectory, @"messages\in", a.Id + ".jpg");
        }

        private Task InsertToBeDeliveredMessage(AS4Message as4Message)
        {
            IPMode pmode = CreateReceivedPMode(
                deliverMessageLocation: DeliveryRoot,
                deliverPayloadLocation: DeliveryRoot);

            return InsertInMessage(
                CreateInMessageRepresentingUserMessage(as4Message, pmode));
        }

        private async Task InsertInMessage(InMessage createInMessageFrom)
        {
            var context = new DatastoreContext(_as4Msh.GetConfiguration());
            var repository = new DatastoreRepository(context);

            repository.InsertInMessage(createInMessageFrom);

            await context.SaveChangesAsync();
        }

        private static InMessage CreateInMessageRepresentingUserMessage(AS4Message as4Message, IPMode pmode)
        {
            var inMessage = new InMessage(as4Message.GetPrimaryMessageId())
            {
                ContentType = as4Message.ContentType,
                MessageLocation =
                    Registry.Instance
                            .MessageBodyStore
                            .SaveAS4Message(@"file:///.\database\as4messages\in", as4Message)
            };

            inMessage.EbmsMessageType = MessageType.UserMessage;
            inMessage.MEP = MessageExchangePattern.Push;
            inMessage.Operation = Operation.ToBeDelivered;

            inMessage.SetPModeInformation(pmode);

            return inMessage;
        }

        private static ReceivingProcessingMode CreateReceivedPMode(
            string deliverMessageLocation,
            string deliverPayloadLocation)
        {
            return new ReceivingProcessingMode
            {
                Id = "DeliverAgent_ReceivingPMode",
                MessageHandling =
                {
                    DeliverInformation =
                    {
                        IsEnabled = true,
                        DeliverMethod = new Method
                        {
                            Type = "FILE",
                            Parameters = new List<Parameter>
                            {
                                new Parameter { Name = "Location", Value = deliverMessageLocation }
                            }
                        },
                        PayloadReferenceMethod = new Method
                        {
                            Type = "FILE",
                            Parameters = new List<Parameter>
                            {
                                new Parameter { Name = "Location", Value = deliverPayloadLocation }
                            }
                        }
                    }
                }
            };
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }
    }
}