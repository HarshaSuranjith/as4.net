﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using MimeKit.IO;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    /// <summary>
    /// <see cref="ISigningStrategy"/> implementation
    /// Responsible for the Signing of the <see cref="AS4Message"/>
    /// </summary>
    internal class SigningStrategy : SignedXml, ISigningStrategy
    {
        private const string CidPrefix = "cid:";
        private readonly string _securityTokenReferenceNamespace;
        private readonly string[] _allowedIdNodeNames;
        private readonly XmlDocument _document;

        public ArrayList References => base.Signature.SignedInfo.References;
        public SecurityTokenReference SecurityTokenReference { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningStrategy"/> class. 
        /// Create Security Header with a given Envelope Document
        /// </summary>
        /// <param name="document">
        /// </param>
        internal SigningStrategy(XmlDocument document) : base(document)
        {
            this._document = document;
            this._allowedIdNodeNames = new[] { "Id", "id", "ID" };
            this._securityTokenReferenceNamespace = $"{Constants.Namespaces.WssSecuritySecExt} SecurityTokenReference";
            this.SignedInfo.CanonicalizationMethod = XmlDsigExcC14NTransformUrl;
        }

        /// <summary>
        /// Add Certificate to the Security Header
        /// </summary>
        /// <param name="certificate"></param>
        public void AddCertificate(X509Certificate2 certificate)
        {
            this.SigningKey = GetSigningKeyFromCertificate(certificate);
            this.KeyInfo = new KeyInfo();

            this.SecurityTokenReference.Certificate = certificate;
            this.KeyInfo.AddClause(this.SecurityTokenReference);
        }

        private RSACryptoServiceProvider GetSigningKeyFromCertificate(X509Certificate2 certificate)
        {
            var cspParams = new CspParameters(24) { KeyContainerName = "XML_DISG_RSA_KEY" };
            var key = new RSACryptoServiceProvider(cspParams);
            using (certificate.GetRSAPrivateKey()) { }
            string keyXml = certificate.PrivateKey.ToXmlString(true);
            key.FromXmlString(keyXml);

            return key;
        }

        /// <summary>
        /// Get Element with a ID Attribute
        /// </summary>
        /// <param name="document"></param>
        /// <param name="idValue"></param>
        /// <returns></returns>
        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            XmlElement idElement = base.GetIdElement(document, idValue);
            if (idElement != null) return idElement;

            foreach (string idNodeName in this._allowedIdNodeNames)
            {
                List<XmlElement> matchingNodes = FindIdElements(document, idValue, idNodeName);
                if (MatchingNodesIsNotPopulated(matchingNodes)) continue;
                idElement = matchingNodes.Single();
                break;
            }

            return idElement;
        }

        private List<XmlElement> FindIdElements(XmlNode document, string idValue, string idNodeName)
        {
            string xpath = $"//*[@*[local-name()='{idNodeName}' and " +
                           $"namespace-uri()='{Constants.Namespaces.WssSecurityUtility}' and .='{idValue}']]";

            return document.SelectNodes(xpath).Cast<XmlElement>().ToList();
        }

        private bool MatchingNodesIsNotPopulated(IReadOnlyCollection<XmlElement> matchingNodes)
        {
            if (matchingNodes.Count <= 0) return true;
            if (matchingNodes.Count >= 2)
                throw new CryptographicException("Malformed reference element.");

            return false;
        }

        /// <summary>
        /// Adds an xml reference.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hashFunction"></param>
        public void AddXmlReference(string id, string hashFunction)
        {
            var reference = new CryptoReference("#" + id) {DigestMethod = hashFunction};
            Transform transform = new XmlDsigExcC14NTransform();
            reference.AddTransform(transform);
            base.AddReference(reference);
        }

        /// <summary>
        /// Add Cic Attachment Reference
        /// </summary>
        /// <param name="attachment"></param>
        /// <param name="digestMethod"></param>
        public void AddAttachmentReference(Attachment attachment, string digestMethod)
        {
            var attachmentReference = new CryptoReference(uri: CidPrefix + attachment.Id) { DigestMethod = digestMethod };
            attachmentReference.AddTransform(new AttachmentSignatureTransform());
            base.AddReference(attachmentReference);
            SetReferenceStream(attachmentReference, attachment);
            SetAttachmentTransformContentType(attachmentReference, attachment);
            ResetReferenceStreamPosition(attachmentReference);
        }

        /// <summary>
        /// Sets the stream of a SignedInfo reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="attachment"></param>
        private void SetReferenceStream(CryptoReference reference, Attachment attachment)
        {
            // We need reflection to set these 2 types. They are implicitly set to Xml references, 
            // but this causes problems with cid: references, since they're not part of the original stream.
            // If performance is slow on this, we can investigate the Delegate.CreateDelegate method to speed things up, 
            // however keep in mind that the reference object changes with every call, so we can't just keep the same delegate and call that.
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo fieldInfo = typeof(CryptoReference).GetField("m_refTargetType", bindingFlags);

            const int streamReferenceTargetType = 0;
            fieldInfo?.SetValue(reference, streamReferenceTargetType);

            fieldInfo = typeof(CryptoReference).GetField("m_refTarget", bindingFlags);
            fieldInfo?.SetValue(reference, new NonCloseableStream(attachment.Content));
        }

        private void SetAttachmentTransformContentType(CryptoReference reference, Attachment attachment)
        {
            foreach (object transform in reference.TransformChain)
            {
                var attachmentTransform = transform as AttachmentSignatureTransform;
                if (attachmentTransform != null)
                    attachmentTransform.ContentType = attachment.ContentType;
            }
        }

        /// <summary>
        /// Resets the reference stream position to 0.
        /// </summary>
        /// <param name="reference"></param>
        private void ResetReferenceStreamPosition(CryptoReference reference)
        {
            FieldInfo fieldInfo = typeof(CryptoReference).GetField(
                "m_refTarget",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null) return;
            var referenceStream = fieldInfo.GetValue(reference) as Stream;

            Stream streamToWorkOn = referenceStream;
            if (streamToWorkOn != null)
            {
                streamToWorkOn.Position = 0;
                if (referenceStream is NonCloseableStream)
                    streamToWorkOn = (referenceStream as NonCloseableStream).InnerStream;
            }
            if (referenceStream is FilteredStream)
                (referenceStream as FilteredStream).Source.Position = 0;
        }

        /// <summary>
        /// Gets the full security XML element.
        /// </summary>
        /// <param name="securityElement"></param>
        public void AppendSignature(XmlElement securityElement)
        {
            LoadSignature();

            AppendSignatureElement(securityElement);
            AddSecurityTokenReferenceToKeyInfo();
            AppendSecurityTokenElements(securityElement);
        }

        private void LoadSignature()
        {
            if (this.References == null || this.References.Count == 0)
                this.LoadXml(GetSignatureElement());
        }

        private void AppendSignatureElement(XmlElement securityElement)
        {
            XmlElement signatureElement = base.GetXml();
            XmlNode importedSignatureElement = securityElement.OwnerDocument.ImportNode(signatureElement, deep: true);
            securityElement.AppendChild(importedSignatureElement);
        }

        private void AddSecurityTokenReferenceToKeyInfo()
        {
            if (!this.KeyInfo.OfType<SecurityTokenReference>().Any() && this.SecurityTokenReference != null)
                this.KeyInfo.AddClause(this.SecurityTokenReference);
        }

        private void AppendSecurityTokenElements(XmlElement securityElement)
        {
            foreach (SecurityTokenReference reference in this.KeyInfo.OfType<SecurityTokenReference>())
                reference.AppendSecurityTokenTo(securityElement, securityElement.OwnerDocument);
        }

        /// <summary>
        /// Returns the public key of a signature.
        /// </summary>
        protected override AsymmetricAlgorithm GetPublicKey()
        {
            AsymmetricAlgorithm publicKey = base.GetPublicKey();
            if (publicKey != null) return publicKey;

            X509Certificate2 signingCertificate = RetrieveSigningCertificate();
            if (signingCertificate != null)
                publicKey = signingCertificate.PublicKey.Key;

            return publicKey;
        }

        private X509Certificate2 RetrieveSigningCertificate()
        {
            if (this.KeyInfo == null) return null;
            foreach (object keyInfo in this.KeyInfo)
            {
                // Embedded (is this actually allowed?)
                var embeddedCertificate = keyInfo as KeyInfoX509Data;
                if (embeddedCertificate != null && embeddedCertificate.Certificates.Count > 0)
                    return embeddedCertificate.Certificates[0] as X509Certificate2;

                // Reference
                var securityTokenReference = keyInfo as SecurityTokenReference;
                if (securityTokenReference != null)
                    return securityTokenReference.Certificate;
            }
            return null;
        }

        /// <summary>
        /// Add Algorithm to the Security Header
        /// </summary>
        /// <param name="algorithm"></param>
        public void AddAlgorithm(SignatureAlgorithm algorithm)
        {
            this.SignedInfo.SignatureMethod = algorithm.GetIdentifier();
            this.SafeCanonicalizationMethods.Add(AttachmentSignatureTransform.Url);

            CryptoConfig.AddAlgorithm(algorithm.GetType(), algorithm.GetIdentifier());
            CryptoConfig.AddAlgorithm(typeof(AttachmentSignatureTransform), AttachmentSignatureTransform.Url);
            CryptoConfig.AddAlgorithm(typeof(SecurityTokenReference), this._securityTokenReferenceNamespace);
        }

        /// <summary>
        /// Sign the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        public void SignSignature()
        {
            base.ComputeSignature();
        }

        /// <summary>
        /// Verify the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool VerifySignature(VerifyConfig options)
        {
            LoadCertificate(options.CertificateRepository);

            this.LoadXml(GetSignatureElement());
            this.AddUnreconizedAttachmentReferences(options.Attachments);

            return this.CheckSignature(this.SecurityTokenReference.Certificate, verifySignatureOnly: true);
        }

        private XmlElement GetSignatureElement()
        {
            XmlNode nodeSignature = this._document.SelectSingleNode("//*[local-name()='Signature'] ");
            var xmlSignature = nodeSignature as XmlElement;
            if (nodeSignature == null || xmlSignature == null)
                throw ThrowAS4SignException("Invalid Signature: Signature Tag not found");

            return xmlSignature;
        }

        private void LoadCertificate(ICertificateRepository certificateRepository)
        {
            this.SecurityTokenReference.CertificateRepository = certificateRepository;
            this.SecurityTokenReference.LoadXml(this._document);

            if (!this.SecurityTokenReference.Certificate.Verify())
                throw ThrowAS4SignException("The signing certificate is not trusted");
        }

        private AS4Exception ThrowAS4SignException(string description)
        {
            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0101)
                .Build();
        }

        private void AddUnreconizedAttachmentReferences(ICollection<Attachment> attachments)
        {
            foreach (CryptoReference reference in SecurityHeaderReferences())
            {
                string pureReferenceId = reference.Uri.Substring(CidPrefix.Length);
                Attachment attachment = attachments.FirstOrDefault(x => x.Id.Equals(pureReferenceId));
                this.SetReferenceStream(reference, attachment);
                this.SetAttachmentTransformContentType(reference, attachment);
            }
        }

        private IEnumerable<CryptoReference> SecurityHeaderReferences()
        {
            return this.SignedInfo.References
                .Cast<CryptoReference>()
                .Where(ReferenceIsCicReference());
        }

        private Func<CryptoReference, bool> ReferenceIsCicReference()
        {
            return x => x?.Uri != null &&
                        x.Uri.StartsWith(CidPrefix) &&
                        x.Uri.Length > CidPrefix.Length;
        }
    }
}