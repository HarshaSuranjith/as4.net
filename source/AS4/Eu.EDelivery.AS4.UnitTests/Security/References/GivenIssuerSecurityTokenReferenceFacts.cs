﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    /// <summary>
    /// Testing <see cref="IssuerSecurityTokenReference" />
    /// </summary>
    public class GivenIssuerSecurityTokenReferenceFacts
    {
        private readonly IssuerSecurityTokenReference _reference;
        private readonly X509Certificate2 _dummyCertificate;

        public GivenIssuerSecurityTokenReferenceFacts()
        {
            this._dummyCertificate = new StubCertificateRepository().GetDummyCertificate();
            this._reference = new IssuerSecurityTokenReference {Certificate = this._dummyCertificate};
        }

        protected XmlElement GetDummyXml()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            XmlElement securityTokenReferenceElement = xmlDocument
                .CreateElement("SecurityTokenReference", Constants.Namespaces.WssSecuritySecExt);
            XmlElement x509DataElement = Createx509DataElement(xmlDocument, securityTokenReferenceElement);
            XmlElement x509IssuerSerialElement = Createx509IssuerSerialElement(xmlDocument, x509DataElement);
            Createx509IssuerNameElement(xmlDocument, x509IssuerSerialElement);
            Createx509SerialNumberElement(xmlDocument, x509IssuerSerialElement);

            return securityTokenReferenceElement;
        }

        private void Createx509SerialNumberElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509SerialNumberElement = xmlDocument.CreateElement("X509SerialNumber");
            x509IssuerSerialElement.AppendChild(x509SerialNumberElement);
            x509SerialNumberElement.InnerText = this._dummyCertificate.SerialNumber;
        }

        private void Createx509IssuerNameElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509IssuerNameElement = xmlDocument.CreateElement("X509IssuerName");
            x509IssuerSerialElement.AppendChild(x509IssuerNameElement);
            x509IssuerNameElement.InnerText = this._dummyCertificate.IssuerName.ToString();
        }

        private XmlElement Createx509IssuerSerialElement(XmlDocument xmlDocument, XmlElement x509DataElement)
        {
            XmlElement x509IssuerSerialElement = xmlDocument.CreateElement("X509IssuerSerial");
            x509DataElement.AppendChild(x509IssuerSerialElement);
            return x509IssuerSerialElement;
        }

        private XmlElement Createx509DataElement(XmlDocument xmlDocument, XmlElement securityTokenReferenceElement)
        {
            XmlElement x509DataElement = xmlDocument.CreateElement("X509Data");
            securityTokenReferenceElement.AppendChild(x509DataElement);
            return x509DataElement;
        }

        /// <summary>
        /// Testing the Reference with valid Arguments for the "GetXml" Method
        /// </summary>
        public class GivenValidArgumentsForGetXml : GivenIssuerSecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenGetXmlContainsSecurityTokenReference()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("wsse:SecurityTokenReference", xmlElement.Name);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.NamespaceURI);
            }

            [Fact]
            public void ThenGetXmlContainsX509Data()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("ds:X509Data", xmlElement.FirstChild.Name);
            }

            [Fact]
            public void ThenGetXmlContainsX509IssuerName()
            {
                // Arrange
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("ds:X509IssuerName", xmlElement.FirstChild.FirstChild.FirstChild.Name);
                Assert.Equal(
                    base._dummyCertificate.IssuerName.Name,
                    xmlElement.FirstChild.FirstChild.FirstChild.InnerText);
            }

            [Fact]
            public void ThenGetXmlContainsX509IssuerSerial()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("ds:X509IssuerSerial", xmlElement.FirstChild.FirstChild.Name);
            }

            [Fact]
            public void ThenGetXmlContainsX509SerialNumber()
            {
                // Arrange
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("ds:X509SerialNumber", xmlElement.FirstChild.FirstChild.ChildNodes[1].Name);

                string expectedSerialNumber = Convert.ToUInt64($"0x{base._dummyCertificate.SerialNumber}", 16).ToString();
                string actualSerialNumber = xmlElement.FirstChild.FirstChild.ChildNodes[1].InnerText;
                Assert.Equal(expectedSerialNumber, actualSerialNumber);
            }
        }

        /// <summary>
        /// Testing the Reference with vlaid Arguments for the "LoadXml" Method
        /// </summary>
        public class GivenValidArgumentsForLoadXml : GivenIssuerSecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenLoadXmlGetsTheCertificateFromTheXml()
            {
                // Arrange
                base._reference.Certificate = null;
                base._reference.CertificateRepository = new StubCertificateRepository();
                XmlElement dummyXml = base.GetDummyXml();
                // Act
               base._reference.LoadXml(dummyXml);
                // Assert
                Assert.Equal(base._dummyCertificate, base._reference.Certificate);
            }
        }
    }
}