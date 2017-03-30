﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Internal;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Security
{
    /// <summary>
    /// Testing the <see cref="SigningStrategyBuilder" />
    /// </summary>
    public class GivenSigningStrategyBuilderFacts
    {
        private SigningStrategyBuilder _builder;

        /// <summary>
        /// Testing if the Builder Succeeds
        /// </summary>
        public class GivenValidArgumentsBuilder : GivenSigningStrategyBuilderFacts
        {
            [Fact]
            public void ThenBuilderLoadsInfoFromXmlDocument()
            {
                // Arrange
                string algorithmNamespace = Constants.Algoritms.First();
                var xmlDocument = new XmlDocument();
                string xml = Properties.Resources.as4_soap_signed_message;
                xmlDocument.LoadXml(xml);

                // Act
                _builder = new SigningStrategyBuilder(xmlDocument);
                ISigningStrategy signingStrategy = _builder.Build();

                // Assert
                var concreteStrategy = signingStrategy as SigningStrategy;
                Assert.IsType<BinarySecurityTokenReference>(concreteStrategy?.SecurityTokenReference);

                var signedXml = signingStrategy as SignedXml;
                Assert.NotNull(signedXml);
                Assert.Equal(algorithmNamespace, signedXml.SignedInfo.SignatureMethod);
            }

            [Fact]
            public void ThenBuilderMakesValidEmptySignStrategy()
            {
                // Arrange
                _builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);

                // Act
                ISigningStrategy signingStrategy = _builder.Build();

                // Assert
                Assert.NotNull(signingStrategy);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithAttachmentReference()
            {
                // Arrange
                _builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes("Dummy Content"));
                var attachment = new Attachment("earth") {Content = stream};
                string hashFunction = Constants.HashFunctions.First();

                // Act
                ISigningStrategy signingStrategy = _builder.WithAttachment(attachment, hashFunction).Build();

                // Assert
                IEnumerable<CryptoReference> references = signingStrategy.GetSignedReferences().Cast<CryptoReference>();
                AssertReference("cid:" + attachment.Id, references);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithCertificate()
            {
                // Arrange
                _builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                X509Certificate2 certificate = new StubCertificateRepository().GetDummyCertificate();

                // Act
                ISigningStrategy signingStrategy =
                    _builder.WithSecurityTokenReference(X509ReferenceType.BSTReference)
                            .WithCertificate(certificate)
                            .Build();

                // Assert
                var concreteStrategy = signingStrategy as SigningStrategy;
                Assert.NotNull(concreteStrategy?.SecurityTokenReference.Certificate);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithSecurityTokenReference()
            {
                // Arrange
                _builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);

                // Act
                ISigningStrategy signingStrategy =
                    _builder.WithSecurityTokenReference(X509ReferenceType.BSTReference).Build();

                // Assert
                Assert.NotNull(signingStrategy);
                var concreteStrategy = signingStrategy as SigningStrategy;
                Assert.NotNull(concreteStrategy?.SecurityTokenReference);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithSignatureAlgorithm()
            {
                // Arrange
                _builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                string algorithmNamespace = Constants.Algoritms.First();

                // Act
                ISigningStrategy signingStrategy = _builder.WithSignatureAlgorithm(algorithmNamespace).Build();

                // Assert
                Assert.NotNull(signingStrategy);
                var signedXml = signingStrategy as SignedXml;
                Assert.NotNull(signedXml);
                Assert.Equal(algorithmNamespace, signedXml.SignedInfo.SignatureMethod);
            }

            [Fact]
            public void ThenBuilerMakesValidSignStrategyWithSigningId()
            {
                // Arrange
                _builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                var signingId = new SigningId("header-id", "body-id");
                string hashFunction = Constants.HashFunctions.First();

                // Act
                ISigningStrategy signingStrategy = _builder.WithSigningId(signingId, hashFunction).Build();

                // Assert
                IEnumerable<CryptoReference> references = signingStrategy.GetSignedReferences().Cast<CryptoReference>();
                AssertReference("#" + signingId.HeaderSecurityId, references);
                AssertReference("#" + signingId.BodySecurityId, references);
            }

            private static void AssertReference(string uri, IEnumerable<CryptoReference> references)
            {
                string hashFunction = Constants.HashFunctions.First();
                CryptoReference reference =
                    references.FirstOrDefault(r => r.Uri.Equals(uri) && r.DigestMethod.Equals(hashFunction));

                Assert.NotNull(reference);
            }
        }

        public class GivenInvalidArguments : GivenSigningStrategyBuilderFacts
        {
            [Fact]
            public void ThenBuilderFailsWithMissingSecurityTokenReferenceXmlElement()
            {
                // Arrange
                string xml = "<?xml version=\"1.0\" encoding=\"utf - 8]\"?>"
                             + $"<s12:Envelope xmlns:s12=\"{Constants.Namespaces.Soap12}\"></s12:Envelope>";

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);

                // Act / Assert
                Assert.Throws<AS4Exception>(() => _builder = new SigningStrategyBuilder(xmlDocument));
            }
        }

        protected Stream GetEnvelopeStream()
        {
            var memoryStream = new MemoryStream();
            var soapEnvelopeBuilder = new SoapEnvelopeBuilder();
            XmlDocument xmlDocument = soapEnvelopeBuilder.Build();
            xmlDocument.Save(memoryStream);

            return memoryStream;
        }
    }
}