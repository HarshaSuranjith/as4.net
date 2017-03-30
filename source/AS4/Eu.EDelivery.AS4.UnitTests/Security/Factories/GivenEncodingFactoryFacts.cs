﻿using System;
using System.Reflection;
using System.Xml;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Factories;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Factories
{
    /// <summary>
    /// Testing <see cref="EncodingFactory" />
    /// </summary>
    public class GivenEncodingFactoryFacts
    {
        private static readonly FieldInfo Mgf1HashProperty;

        static GivenEncodingFactoryFacts()
        {
            Mgf1HashProperty = typeof(OaepEncoding).GetField(
                "mgf1Hash",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public class GivenValidArguments : GivenEncodingFactoryFacts
        {
            [Fact]
            public void ThenCreateEncodingWithDefaultsSucceeds()
            {
                // Act
                OaepEncoding encoding = EncodingFactory.Instance.Create();

                // Assert
                Assert.Equal("RSA/OAEPPadding", encoding.AlgorithmName);
                AssertMgf1Hash(encoding, "SHA-1");
            }

            [Fact]
            public void ThenCreateEncodingWithGivenAlgorithmSucceeds()
            {
                // Arrange
                const string algorithmName = "http://www.w3.org/2001/04/xmlenc#sha256";

                // Act
                OaepEncoding encoding = EncodingFactory.Instance.Create(algorithmName);

                // Assert
                Assert.Equal("RSA/OAEPPadding", encoding.AlgorithmName);
                AssertMgf1Hash(encoding, "SHA-1");
            }

            [Fact]
            public void ThenCreateEncodingWithGivenMgfSucceeds()
            {
                const string mgfAlgorithmName = "http://www.w3.org/2009/xmlenc11#mgf1sha256";

                OaepEncoding encoding = EncodingFactory.Instance.Create(null, mgfAlgorithmName);

                // Assert
                Assert.Equal("RSA/OAEPPadding", encoding.AlgorithmName);
                AssertMgf1Hash(encoding, "SHA-256");
            }
        }

        public class GivenValidEncryptedKeyXmlDocument : GivenEncodingFactoryFacts
        {
            [Fact]
            public void ThenCreateCorrectEncoding()
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.EncryptedKeyWithMGFSpec);

                // Act
                AS4EncryptedKey as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(xmlDocument);

                OaepEncoding encoding = EncodingFactory.Instance.Create(
                    as4EncryptedKey.GetDigestAlgorithm(),
                    as4EncryptedKey.GetMaskGenerationFunction());

                Assert.NotNull(encoding);
                AssertMgf1Hash(encoding, "SHA-256");
            }
        }

        protected void AssertMgf1Hash(OaepEncoding encoding, string expectedValue)
        {
            if (Mgf1HashProperty == null) throw new ArgumentException("Field mgf1Hash could not be found in OaepEncoding type.");

            var digest = Mgf1HashProperty.GetValue(encoding) as IDigest;
            Assert.True(StringComparer.OrdinalIgnoreCase.Equals(expectedValue, digest.AlgorithmName));
        }
    }
}