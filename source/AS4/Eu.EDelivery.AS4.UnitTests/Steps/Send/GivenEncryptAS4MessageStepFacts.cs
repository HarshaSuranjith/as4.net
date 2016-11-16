﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="EncryptAS4MessageStep" />
    /// </summary>
    public class GivenEncryptAS4MessageStepFacts
    {
        private EncryptAS4MessageStep _step;

        public GivenEncryptAS4MessageStepFacts()
        {
            var certificateRepositoryMock = new Mock<ICertificateRepository>();

            certificateRepositoryMock
                .Setup(x => x.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                .Returns(new StubCertificateRepository().GetDummyCertificate());

            this._step = new EncryptAS4MessageStep(certificateRepositoryMock.Object);
        }


        public class GivenValidArguments : GivenEncryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                AS4Message as4Message = base.CreateEncryptedAS4Message();
                var internalMessage = new InternalMessage(as4Message);
                // Act
                StepResult stepResult = await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Assert.True(stepResult.InternalMessage.AS4Message.IsEncrypted);
            }
        }

        public class GivenInvalidArguments : GivenEncryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithInvalidCertificateAsync()
            {
                // Arrange
                AS4Message as4Message = base.CreateEncryptedAS4Message();
                var internalMessage = new InternalMessage(as4Message);

                Mock<ICertificateRepository> certificateRepositoryMock = CreateFailedMockedCertificateRepository();
                base._step = new EncryptAS4MessageStep(certificateRepositoryMock.Object);

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => base._step.ExecuteAsync(internalMessage, CancellationToken.None));
            }

            private Mock<ICertificateRepository> CreateFailedMockedCertificateRepository()
            {
                var certificateRepositoryMock = new Mock<ICertificateRepository>();

                certificateRepositoryMock
                    .Setup(x => x.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                    .Throws(new Exception("Invalid Certificate"));

                return certificateRepositoryMock;
            }
        }

        protected Task<AS4Message> GetEncryptedAS4MessageAsync()
        {
            Stream inputStream = new MemoryStream(Properties.Resources.as4_encrypted_message);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            return serializer.DeserializeAsync(inputStream,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"",
                CancellationToken.None);
        }

        protected AS4Message CreateEncryptedAS4Message()
        {
            Stream attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, encrypt me"));
            var attachment = new Attachment(id:  "attachment-id") {Content = attachmentStream};
            AS4Message as4Message = new AS4MessageBuilder()
                .WithAttachment(attachment).Build();

            as4Message.SendingPMode = new SendingProcessingMode();
            as4Message.SendingPMode.Security.Encryption.IsEnabled = true;
            as4Message.SendingPMode.Security.Encryption.Algorithm = "http://www.w3.org/2009/xmlenc11#aes128-gcm";

            return as4Message;
        }
    }
}
