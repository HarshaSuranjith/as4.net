﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="DecompressAttachmentsStep" />
    /// </summary>
    public class GivenDecompressAttachmentsStepFacts
    {
        public class GivenValidArguments : GivenDecompressAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteSucceedsWithValidAttachmentsAsync()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();

                // Act
                StepResult stepResult = await ExerciseDecompress(context);

                // Assert
                Assert.NotNull(stepResult.MessagingContext.AS4Message.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNoCompressedAttachmentAsync()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();
                context.AS4Message.Attachments.First().ContentType = "not supported MIME type";

                // Act
                StepResult stepResult = await ExerciseDecompress(context);

                // Assert
                Assert.All(
                    stepResult.MessagingContext.AS4Message.Attachments,
                    a => Assert.NotEqual("application/gzip", a.ContentType));
            }
        }

        public class GivenInvalidArguments : GivenDecompressAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteFailsWithMissingMimTypePartPropertyAsync()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();
                Attachment attachment = context.AS4Message.Attachments.First();
                attachment.Properties.Remove("MimeType");

                // Act
                StepResult result = await ExerciseDecompress(context);
                
                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0303, error.Code);
            }
        }

        private static MessagingContext CompressedAS4Message()
        {
            const string attachmentId = "attachment-id";

            AS4Message as4Message = AS4Message.Create(UserMessageWithCompressedInfo(attachmentId));
            as4Message.AddAttachment(CompressedAttachment(attachmentId));

            return new MessagingContext(as4Message, MessagingContextMode.Unknown);
        }

        private static Attachment CompressedAttachment(string attachmentId)
        {
            Attachment attachment = CreateAttachment(attachmentId);
            CompressAttachment(attachment);
            AssignAttachmentProperties(attachment);

            return attachment;
        }

        private static Attachment CreateAttachment(string id)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Dummy Attachment Content");
            return new Attachment(id, new MemoryStream(bytes), "text/plain");
        }

        private static void CompressAttachment(Attachment attachment)
        {
            var memoryStream = new MemoryStream();
            var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
            attachment.Content.CopyTo(gzipStream);

            memoryStream.Position = 0;
            attachment.Content = memoryStream;
        }

        private static void AssignAttachmentProperties(Attachment attachment)
        {
            attachment.ContentType = "application/gzip";
            attachment.Properties["MimeType"] = "html/text";
        }

        private static UserMessage UserMessageWithCompressedInfo(string attachmentId)
        {
            var properties = new Dictionary<string, string> { ["MimeType"] = "html/text" };
            var partInfo = new PartInfo("cid:" + attachmentId, properties, new Schema[0]);
            var userMessage = new UserMessage("message-id");
            userMessage.AddPartInfo(partInfo);

            return userMessage;
        }

        private static async Task<StepResult> ExerciseDecompress(MessagingContext context)
        {
            var sut = new DecompressAttachmentsStep();

            // Act
            return await sut.ExecuteAsync(context);
        }
    }
}