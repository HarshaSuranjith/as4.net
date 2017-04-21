﻿using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Security Token Reference Strategy for the Key Identifier
    /// </summary>
    internal class KeyIdentifierSecurityTokenReference : SecurityTokenReference
    {
        private readonly string _keyInfoId;
        private readonly ICertificateRepository _certificateReposistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyIdentifierSecurityTokenReference"/> class. 
        /// </summary>
        /// <param name="certificateRepository">Repository to obtain the certificate needed to embed it into the Key Identifier Security Token Reference.</param>
        public KeyIdentifierSecurityTokenReference(ICertificateRepository certificateRepository)
        {
            _certificateReposistory = certificateRepository;
            _keyInfoId = $"KI-{Guid.NewGuid()}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyIdentifierSecurityTokenReference"/> class.
        /// </summary>
        /// <param name="envelope">SOAP Envelope with a Key Identifier Security Token Reference.</param>
        /// <param name="certificateRepository">Repository to obtain the certificate needed to embed it into the Key Identifier Security Token Reference.</param>
        public KeyIdentifierSecurityTokenReference(XmlElement envelope, ICertificateRepository certificateRepository)
        {
            _certificateReposistory = certificateRepository;
            LoadXml(envelope);
        }

        /// <summary>
        /// Load the <see cref="X509Certificate2" />
        /// from the given <paramref name="element" />
        /// </summary>
        /// <param name="element"></param>
        public override sealed void LoadXml(XmlElement element)
        {
            var xmlKeyIdentifier = element.SelectSingleNode(".//*[local-name()='KeyIdentifier']") as XmlElement;
            if (xmlKeyIdentifier == null)
            {
                throw new AS4Exception("No KeyIdentifier tag found in given XmlElement");
            }

            SoapHexBinary soapHexBinary = RetrieveHexBinaryFromKeyIdentifier(xmlKeyIdentifier);
            SaveCertificateWithHexBinary(soapHexBinary);
        }

        private static SoapHexBinary RetrieveHexBinaryFromKeyIdentifier(XmlNode xmlKeyIdentifier)
        {
            byte[] base64Bytes = Convert.FromBase64String(xmlKeyIdentifier.InnerText);
            return new SoapHexBinary(base64Bytes);
        }

        private void SaveCertificateWithHexBinary(SoapHexBinary soapHexBinary)
        {
            Certificate = _certificateReposistory.GetCertificate(
                X509FindType.FindBySubjectKeyIdentifier,
                soapHexBinary.ToString());
        }

        /// <summary>
        /// Add a KeyInfo Id to the <KeyInfo/> Element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public override XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            var nodeKeyInfo = (XmlElement)element.SelectSingleNode("//*[local-name()='KeyInfo']");
            nodeKeyInfo?.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, this._keyInfoId);

            return element;
        }

        /// <summary>
        /// Get the Xml for the Key Identifier
        /// </summary>
        /// <returns></returns>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument {PreserveWhitespace = true};

            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                prefix: "wsse",
                localName: "SecurityTokenReference",
                namespaceURI: Constants.Namespaces.WssSecuritySecExt);

            XmlElement keyIdentifierElement = xmlDocument.CreateElement(
                prefix: "wsse",
                localName: "KeyIdentifier",
                namespaceURI: Constants.Namespaces.WssSecuritySecExt);

            SetKeyIfentifierSecurityAttributes(keyIdentifierElement);
            keyIdentifierElement.InnerText = GetSubjectKeyIdentifier();
            securityTokenReferenceElement.AppendChild(keyIdentifierElement);

            return securityTokenReferenceElement;
        }

        private static void SetKeyIfentifierSecurityAttributes(XmlElement keyIdentifierElement)
        {
            keyIdentifierElement.SetAttribute("EncodingType", Constants.Namespaces.Base64Binary);
            keyIdentifierElement.SetAttribute("ValueType", Constants.Namespaces.SubjectKeyIdentifier);
        }

        private string GetSubjectKeyIdentifier()
        {
            foreach (X509Extension extension in Certificate.Extensions)
            {
                if (IsExtensionNotSubjectKeyIdentifier(extension))
                {
                    continue;
                }

                return RetrieveBinary64SubjectKeyIdentifier(extension);
            }

            return string.Empty;
        }

        private static string RetrieveBinary64SubjectKeyIdentifier(X509Extension extension)
        {
            var x509SubjectKeyIdentifierExtension = (X509SubjectKeyIdentifierExtension)extension;
            SoapHexBinary base64Binary = SoapHexBinary.Parse(x509SubjectKeyIdentifierExtension.SubjectKeyIdentifier);

            return Convert.ToBase64String(base64Binary.Value);
        }

        private static bool IsExtensionNotSubjectKeyIdentifier(AsnEncodedData extension)
        {
            return !string.Equals(extension.Oid.FriendlyName, "Subject Key Identifier");
        }
    }
}