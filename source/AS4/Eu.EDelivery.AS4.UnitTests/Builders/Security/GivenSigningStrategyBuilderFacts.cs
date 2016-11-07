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
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Security
{
    /// <summary>
    /// Testing the <see cref="SigningStrategyBuilder"/>
    /// </summary>
    public class GivenSigningStrategyBuilderFacts
    {
        private SigningStrategyBuilder _builder;

        protected Stream GetEnvelopeStream()
        {
            var memoryStream = new MemoryStream();
            var soapEnvelopeBuilder = new SoapEnvelopeBuilder();
            XmlDocument xmlDocument = soapEnvelopeBuilder.Build();
            xmlDocument.Save(memoryStream);

            return memoryStream;
        }

        /// <summary>
        /// Testing if the Builder Succeeds
        /// </summary>
        public class GivenValidArgumentsBuilder : GivenSigningStrategyBuilderFacts
        {
            [Fact]
            public void ThenBuilderMakesValidEmptySignStrategy()
            {
                // Arrange
                base._builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                // Act
                ISigningStrategy signingStrategy = base._builder.Build();
                // Assert
                Assert.NotNull(signingStrategy);
            }

            [Fact]
            public void ThenBuilderLoadsInfoFromXmlDocument()
            {
                // Arrange
                string algorithmNamespace = Constants.Algoritms.First();
                var xmlDocument = new XmlDocument();
                string xml = Properties.Resources.as4_soap_signed_message;
                xmlDocument.LoadXml(xml);
                
                // Act
                base._builder = new SigningStrategyBuilder(xmlDocument);
                ISigningStrategy signingStrategy = base._builder.Build();
                
                // Assert
                Assert.IsType<BinarySecurityTokenReference>(signingStrategy.SecurityTokenReference);

                var signedXml = signingStrategy as SignedXml;
                Assert.NotNull(signedXml);
                Assert.Equal(algorithmNamespace, signedXml.SignedInfo.SignatureMethod);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithSecurityTokenReference()
            {
                // Arrange
                base._builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);

                // Act
                ISigningStrategy signingStrategy = base._builder
                    .WithSecurityTokenReference(X509ReferenceType.BSTReference).Build();

                // Assert
                Assert.NotNull(signingStrategy);
                Assert.NotNull(signingStrategy.SecurityTokenReference);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithSignatureAlgorithm()
            {
                // Arrange
                base._builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                string algorithmNamespace = Constants.Algoritms.First();

                // Act
                ISigningStrategy signingStrategy = base._builder
                    .WithSignatureAlgorithm(algorithmNamespace).Build();

                // Assert
                Assert.NotNull(signingStrategy);
                var signedXml = signingStrategy as SignedXml;
                Assert.NotNull(signedXml);
                Assert.Equal(algorithmNamespace, signedXml.SignedInfo.SignatureMethod);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithCertificate()
            {
                // Arrange
                base._builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                X509Certificate2 certificate = new StubCertificateRepository().GetDummyCertificate();
                
                // Act
                ISigningStrategy signingStrategy = base._builder
                    .WithSecurityTokenReference(X509ReferenceType.BSTReference)
                    .WithCertificate(certificate)
                    .Build();
                
                // Assert
                Assert.NotNull(signingStrategy.SecurityTokenReference.Certificate);
            }

            [Fact]
            public void ThenBuilderMakesValidSignStrategyWithAttachmentReference()
            {
                // Arrange
                base._builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes("Dummy Content"));
                var attachment = new Attachment(id: "earth") {Content = stream};
                string hashFunction = Constants.HashFunctions.First();
                
                // Act
                ISigningStrategy signingStrategy = base._builder
                    .WithAttachment(attachment, hashFunction).Build();
                
                // Assert
                IEnumerable<CryptoReference> references = signingStrategy.References.Cast<CryptoReference>();
                AssertReference("cid:" + attachment.Id, references);
            }

            [Fact]
            public void ThenBuilerMakesValidSignStrategyWithSigningId()
            {
                // Arrange
                base._builder = new SigningStrategyBuilder(new AS4Message(), CancellationToken.None);
                var signingId = new SigningId("header-id", "body-id");
                string hashFunction = Constants.HashFunctions.First();
                
                // Act
                ISigningStrategy signingStrategy = base._builder
                    .WithSigningId(signingId, hashFunction).Build();
                
                // Assert
                IEnumerable<CryptoReference> references = signingStrategy.References.Cast<CryptoReference>();
                AssertReference("#" + signingId.HeaderSecurityId, references);
                AssertReference("#" + signingId.BodySecurityId, references);
            }

            private void AssertReference(string uri, IEnumerable<CryptoReference> references)
            {
                string hashFunction = Constants.HashFunctions.First();
                CryptoReference reference = references
                    .FirstOrDefault(r => r.Uri.Equals(uri) && r.DigestMethod.Equals(hashFunction));

                Assert.NotNull(reference);
            }
        }

        public class GivenInvalidArguments : GivenSigningStrategyBuilderFacts
        {
            [Fact]
            public void ThenBuilderFailsWithMissingSecurityTokenReferenceXmlElement()
            {
                // Arrange
                const string xml = "<?xml version=\"1.0\" encoding=\"utf - 8]\"?>" +
                                   "<s12:Envelope xmlns:s12=\"http://www.w3.org/2003/05/soap-envelope\"></s12:Envelope>";

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                
                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => base._builder = new SigningStrategyBuilder(xmlDocument));
            }
        }
    }
}
