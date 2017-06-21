﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    /// <summary>
    /// Testing <see cref="SigningStrategy" />
    /// </summary>
    public class GivenSignStrategyFacts
    {       
        public class GivenValidArguments : GivenSignStrategyFacts
        {
            [Fact]
            public void ThenSignStrategySignsCorrectlyAS4Message()
            {
                // Arrange
                ISigningStrategy signingStrategy = CreateSignStrategyForSigning();

                // Act
                signingStrategy.SignSignature();

                // Assert
                XmlElement securityElement = CreateSecurityElement();
                signingStrategy.AppendSignature(securityElement);
                AssertSecurityElement(securityElement);
            }

            private ISigningStrategy CreateSignStrategyForSigning()
            {
                var signingId = new SigningId("header-id", "body-id");
                AS4Message as4Message = AS4Message.Empty;
                as4Message.SigningId = signingId;

                XmlDocument xmlDocument = SerializeAS4Message(as4Message);

                return CreateDefaultSignStrategy(signingId, xmlDocument);
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

            private static SigningStrategy CreateDefaultSignStrategy(SigningId signingId, XmlDocument xmlDocument)
            {
                var signStrategy = new SigningStrategy(xmlDocument, new BinarySecurityTokenReference());

                signStrategy.AddAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());
                signStrategy.AddCertificate(new StubCertificateRepository().GetStubCertificate());
                signStrategy.AddXmlReference(signingId.HeaderSecurityId, Constants.HashFunctions.First());
                signStrategy.AddXmlReference(signingId.BodySecurityId, Constants.HashFunctions.First());

                return signStrategy;
            }

            private static XmlElement CreateSecurityElement()
            {
                var xmlSecurityHeader = new XmlDocument();
                XmlElement securityElement = xmlSecurityHeader.CreateElement(
                    "Security",
                    Constants.Namespaces.WssSecuritySecExt);

                return securityElement;
            }

            private static void AssertSecurityElement(XmlNode securityElement)
            {
                XmlNode xmlSignature = securityElement.SelectSingleNode("//*[local-name()='Signature'] ");
                Assert.NotNull(xmlSignature);

                XmlNodeList xmlReferences = xmlSignature.SelectNodes("//*[local-name()='Reference'] ");
                Assert.NotNull(xmlReferences);
                Assert.True(xmlReferences.Count == 3);
            }

            [Fact]
            public void ThenSignStrategyVerifiesAS4MessageCorrectly()
            {
                // Arrange
                ISigningStrategy signingStrategy = CreateSignStrategyForVerifing();

                // Act
                bool isValid = signingStrategy.VerifySignature(EmptyVerifyConfig());

                // Assert
                Assert.True(isValid);
            }

            private static ISigningStrategy CreateSignStrategyForVerifing()
            {
                XmlDocument xmlDocument = CreateXmlDocument();
                SecurityTokenReference reference = CreateSecurityTokenReference(xmlDocument);

                var signStrategy = new SigningStrategy(xmlDocument, reference);
                signStrategy.AddAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());

                return signStrategy;
            }

            private static XmlDocument CreateXmlDocument()
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(as4_soap_signed_message);

                return xmlDocument;
            }

            private static SecurityTokenReference CreateSecurityTokenReference(XmlNode xmlDocument)
            {
                SecurityTokenReference reference = new BinarySecurityTokenReference();
                XmlNode securityTokenElement =
                    xmlDocument.SelectSingleNode("//*[local-name()='SecurityTokenReference'] ");
                reference.LoadXml((XmlElement)securityTokenElement);

                return reference;
            }
        }

        public class GivenInvalidArgumens : GivenSignStrategyFacts
        {
            [Fact]
            public void ThenVerifySignatureFailsWithUntrustedCertificate()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(as4_soap_untrusted_signed_message);

                SigningStrategy signStrategy = ConfigureDefaultSignStrategy(xmlDocument);

                // Act / Assert
                Assert.Throws<System.Security.Cryptography.CryptographicException>(
                    () => signStrategy.VerifySignature(EmptyVerifyConfig()));
            }

            private static SigningStrategy ConfigureDefaultSignStrategy(XmlDocument document)
            {
                var builder = new SigningStrategyBuilder(document);
                builder.WithSignatureAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());

                return (SigningStrategy)builder.Build();
            }
        }

        protected VerifyConfig EmptyVerifyConfig()
        {
            return new VerifyConfig {Attachments = new List<Attachment>()};
        }
    }
}