﻿using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Uploader;
using Moq;
using Xunit;
using RetryReliability = Eu.EDelivery.AS4.Entities.RetryReliability;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    public class GivenUploadAttachmentsStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task Throws_When_Uploading_Attachments_Failed()
        {
            // Arrange
            var saboteurProvider = new Mock<IAttachmentUploaderProvider>();
            saboteurProvider
                .Setup(p => p.Get(It.IsAny<string>()))
                .Throws(new Exception("Failed to get Uploader"));

            IStep sut = new UploadAttachmentsStep(saboteurProvider.Object, GetDataStoreContext);

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await sut.ExecuteAsync(await CreateAS4MessageWithAttachmentAsync()));
        }

        [Theory]
        [ClassData(typeof(UploadRetryData))]
        public async Task Retries_Uploading_When_Uploader_Returns_RetryableFail_Result(UploadRetry input)
        {
            // Arrange
            string id = "deliver-" + Guid.NewGuid();
            InMessage im = InsertInMessage(id);

            var r = RetryReliability.CreateForInMessage(
                refToInMessageId: im.Id,
                maxRetryCount: input.MaxRetryCount,
                retryInterval: default(TimeSpan),
                type: RetryType.Delivery);
            r.CurrentRetryCount = input.CurrentRetryCount;
            GetDataStoreContext.InsertRetryReliability(r);

            var a = new FilledAttachment();
            var userMessage = new FilledUserMessage(id, a.Id);
            AS4Message as4Msg = AS4Message.Create(userMessage);
            as4Msg.AddAttachment(a);

            MessagingContext fixture = await PrepareAS4MessageForDeliveryAsync(as4Msg, CreateReceivingPModeWithPayloadMethod());

            IAttachmentUploader stub = CreateStubAttachmentUploader(fixture.DeliverMessage.Message.MessageInfo, input.UploadResult);

            // Act
            await CreateUploadStep(stub).ExecuteAsync(fixture);

            // Assert
            GetDataStoreContext.AssertInMessage(id, actual =>
            {
                Assert.NotNull(actual);
                Assert.Equal(input.ExpectedStatus, actual.Status.ToEnum<InStatus>());
                Assert.Equal(input.ExpectedOperation, actual.Operation);
            });
        }

        private static async Task<MessagingContext> PrepareAS4MessageForDeliveryAsync(AS4Message msg, ReceivingProcessingMode pmode)
        {
            var transformer = new DeliverMessageTransformer();

            var entity = new InMessage(msg.GetPrimaryMessageId());
            entity.SetPModeInformation(pmode);

            return await transformer.TransformAsync(new ReceivedEntityMessage(entity, msg.ToStream(), msg.ContentType));
        }

        [Theory]
        [ClassData(typeof(UploadRetryData))]
        public async Task All_Attachments_Should_Succeed_Or_Fail(UploadRetry input)
        {
            // Arrange
            string id = "deliver-" + Guid.NewGuid();
            InMessage im = InsertInMessage(id);

            var r = RetryReliability.CreateForInMessage(
                refToInMessageId: im.Id,
                maxRetryCount: input.MaxRetryCount,
                retryInterval: default(TimeSpan),
                type: RetryType.Delivery);
            r.CurrentRetryCount = input.CurrentRetryCount;
            GetDataStoreContext.InsertRetryReliability(r);


            var a1 = new FilledAttachment("attachment-1");
            var a2 = new FilledAttachment("attachment-2");
            var userMessage = new FilledUserMessage(id, a1.Id, a2.Id);

            var as4Msg = AS4Message.Create(userMessage);
            as4Msg.AddAttachment(a1);
            as4Msg.AddAttachment(a2);

            MessagingContext fixture = await PrepareAS4MessageForDeliveryAsync(as4Msg, CreateReceivingPModeWithPayloadMethod());

            var stub = new Mock<IAttachmentUploader>();
            stub.Setup(s => s.UploadAsync(a1, fixture.DeliverMessage.Message.MessageInfo))
                .ReturnsAsync(input.UploadResult);
            stub.Setup(s => s.UploadAsync(a2, fixture.DeliverMessage.Message.MessageInfo))
                .ReturnsAsync(
                    input.UploadResult.Status == SendResult.Success
                        ? UploadResult.FatalFail
                        : UploadResult.RetryableFail);

            // Act
            await CreateUploadStep(stub.Object).ExecuteAsync(fixture);

            // Assert
            GetDataStoreContext.AssertInMessage(id, actual =>
            {
                Assert.NotNull(actual);
                Operation op = actual.Operation;
                Assert.NotEqual(Operation.Delivered, op);
                InStatus st = actual.Status.ToEnum<InStatus>();
                Assert.NotEqual(InStatus.Delivered, st);

                bool operationToBeRetried = Operation.ToBeRetried == op;
                bool uploadResultCanBeRetried =
                    input.UploadResult.Status == SendResult.RetryableFail
                    && input.CurrentRetryCount < input.MaxRetryCount;

                Assert.True(
                    operationToBeRetried == uploadResultCanBeRetried,
                    "InMessage should update Operation=ToBeDelivered");

                bool messageSetToException = Operation.DeadLettered == op && InStatus.Exception == st;
                bool exhaustRetries =
                    input.CurrentRetryCount == input.MaxRetryCount
                    || input.UploadResult.Status != SendResult.RetryableFail;

                Assert.True(
                    messageSetToException == exhaustRetries,
                    $"{messageSetToException} != {exhaustRetries} InMessage should update Operation=DeadLettered, Status=Exception");
            });
        }

        private InMessage InsertInMessage(string id)
        {
            var inMsg = new InMessage(id);
            inMsg.SetStatus(InStatus.Received);
            inMsg.Operation = Operation.Delivering;

            return GetDataStoreContext.InsertInMessage(inMsg);
        }

        private static IAttachmentUploader CreateStubAttachmentUploader(MessageInfo m, UploadResult r)
        {
            var stub = new Mock<IAttachmentUploader>();
            stub.Setup(s => s.UploadAsync(It.IsAny<Attachment>(), m))
                .ReturnsAsync(r);

            return stub.Object;
        }

        [Fact]
        public async Task Update_With_Attachment_Location_When_Uploading_Attachments_Succeeds()
        {
            // Arrange
            const string expectedLocation = "http://path/to/download/attachment";
            var stubUploader = new StubAttachmentUploader(expectedLocation);

            // Act
            StepResult result =
                await CreateUploadStep(stubUploader)
                    .ExecuteAsync(await CreateAS4MessageWithAttachmentAsync());

            // Assert
            Assert.Collection(
                result.MessagingContext.DeliverMessage.Message.Payloads,
                p => Assert.Equal(expectedLocation, p.Location));
        }

        private static async Task<MessagingContext> CreateAS4MessageWithAttachmentAsync()
        {
            const string attachmentId = "attachment-id";

            var userMessage = new UserMessage(Guid.NewGuid().ToString(), new PartInfo("cid:" + attachmentId));
            AS4Message as4Message = AS4Message.Create(userMessage);
            as4Message.AddAttachment(new Attachment(attachmentId, Stream.Null, "text/plain"));
            ReceivingProcessingMode pMode = CreateReceivingPModeWithPayloadMethod();

            return await PrepareAS4MessageForDeliveryAsync(as4Message, pMode);
        }

        private static ReceivingProcessingMode CreateReceivingPModeWithPayloadMethod()
        {
            return new ReceivingProcessingMode
            {
                MessageHandling =
                {
                    DeliverInformation =
                    {
                        PayloadReferenceMethod = new Method { Type = "FILE" }
                    }
                }
            };
        }

        /// <summary>
        /// Creates the upload step.
        /// </summary>
        /// <param name="uploader">The uploader.</param>
        /// <returns></returns>
        private IStep CreateUploadStep(IAttachmentUploader uploader)
        {
            return new UploadAttachmentsStep(new StubAttachmentUploaderProvider(uploader), GetDataStoreContext);
        }
    }
}