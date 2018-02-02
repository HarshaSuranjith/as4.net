﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class GivenEncryptionStrategyFacts
    {
        public class GivenValidArgumentsForDecryptMessage : GivenEncryptionStrategyFacts
        {
            [Fact]
            public async void ThenDecryptDecryptsTheAttachmentsCorrectly()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedMessageAsync();
                X509Certificate2 decryptCertificate = CreateDecryptCertificate();

                // Act
                as4Message.Decrypt(decryptCertificate);

                // Assert
                Assert.Equal(Properties.Resources.flower1, GetAttachmentContents(as4Message.Attachments.ElementAt(0)));
                Assert.Equal(Properties.Resources.flower2, GetAttachmentContents(as4Message.Attachments.ElementAt(1)));
            }

            [Fact]
            public void ThenEncryptEncryptsTheAttachmentsCorrectly()
            {
                // Arrange
                byte[] attachmentContents = Encoding.UTF8.GetBytes("hi!");
                var attachment = new Attachment("attachment-id") { Content = new MemoryStream(attachmentContents) };

                AS4Message as4Message = AS4Message.Create(pmode: null);
                as4Message.AddAttachment(attachment);

                IEncryptionStrategy encryptionStrategy = CreateEncryptionStrategyForEncrypting(as4Message);

                // Act
                encryptionStrategy.EncryptMessage();

                // Assert
                Attachment firstAttachment = as4Message.Attachments.ElementAt(0);
                Assert.NotEqual(attachmentContents.Length, firstAttachment?.Content.Length);
            }

            private static IEncryptionStrategy CreateEncryptionStrategyForEncrypting(AS4Message message)
            {
                X509Certificate2 certificate = CreateDecryptCertificate();

                return EncryptionStrategyBuilder
                    .Create(message)
                    .WithCertificate(certificate)
                    .Build();
            }
        }

        private static X509Certificate2 CreateDecryptCertificate()
        {
            return new X509Certificate2(
                Properties.Resources.holodeck_partyc_certificate,
                "ExampleC",
                X509KeyStorageFlags.Exportable);
        }

        public class GivenInvalidArgumentsForDecryptMessage : GivenEncryptionStrategyFacts
        {
            [Fact]
            public async Task ThenDecryptThrowsAnAS4Exception()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedMessageAsync();
                var certificate = new StubCertificateRepository().GetStubCertificate();
                
                // Act&Assert
                Assert.ThrowsAny<Exception>(() => as4Message.Decrypt(certificate));
            }

            [Fact]
            public async Task FailsToDecrypt_IfInvalidKeySize()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedMessageAsync();
                var pmode = new SendingProcessingMode { Security = { Encryption = { AlgorithmKeySize = -1 } } };

                EncryptionStrategy sut = EncryptionStrategyFor(as4Message, pmode);

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => sut.EncryptMessage());
            }

            private static EncryptionStrategy EncryptionStrategyFor(AS4Message as4Message, SendingProcessingMode pmode)
            {
                AS4.Model.PMode.Encryption encryption = pmode.Security.Encryption;
                var dataEncryptConfig = new DataEncryptionConfiguration(encryption.Algorithm, algorithmKeySize: encryption.AlgorithmKeySize);

                return EncryptionStrategyBuilder
                    .Create(as4Message)
                    .WithDataEncryptionConfiguration(dataEncryptConfig)
                    .Build();
            }
        }

        protected XmlDocument SerializeAS4Message(AS4Message as4Message)
        {
            var memoryStream = new MemoryStream();
            var provider = new SerializerProvider();
            ISerializer serializer = provider.Get(Constants.ContentTypes.Soap);
            serializer.Serialize(as4Message, memoryStream, CancellationToken.None);

            return LoadEnvelopeToDocument(memoryStream);
        }

        private static XmlDocument LoadEnvelopeToDocument(Stream envelopeStream)
        {
            envelopeStream.Position = 0;
            var envelopeXmlDocument = new XmlDocument();
            var readerSettings = new XmlReaderSettings { CloseInput = false };

            using (XmlReader reader = XmlReader.Create(envelopeStream, readerSettings))
            {
                envelopeXmlDocument.Load(reader);
            }

            return envelopeXmlDocument;
        }

        protected static Task<AS4Message> GetEncryptedMessageAsync()
        {
            Stream inputStream = new MemoryStream(Properties.Resources.as4_encrypted_message);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            return serializer.DeserializeAsync(
                inputStream,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"",
                CancellationToken.None);
        }

        protected byte[] GetAttachmentContents(Attachment attachment)
        {
            var attachmentInMemory = new MemoryStream();
            attachment.Content.CopyTo(attachmentInMemory);

            return attachmentInMemory.ToArray();
        }
    }
}