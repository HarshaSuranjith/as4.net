using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;
using MessageProperty = Eu.EDelivery.AS4.Model.PMode.MessageProperty;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    public class ESensDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly HttpClient HttpClient = new HttpClient();

        private const string DocumentIdentifier = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biitrns010:ver2.0:extended:urn:www.peppol.eu:bis:peppol5a:ver2.0::2.1";
        private const string DocumentIdentifierScheme = "busdox-docid-qns";

        [Info("SML Scheme", defaultValue: "iso6523-actorid-upis")]
        [Description("Used to build the SML Uri")]
        private string SmlScheme { get; }

        [Info("SMP Server Domain Name", defaultValue: "isaitb.acc.edelivery.tech.ec.europa.eu")]
        [Description("Domain name that must be used in the Uri")]
        private string SmpServerDomainName { get; }

        private class ESensConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ESensConfig"/> class.
            /// </summary>
            /// <param name="smlScheme">The SML scheme.</param>
            /// <param name="smpServerDomainName">Name of the SMP server domain.</param>
            private ESensConfig(string smlScheme, string smpServerDomainName)
            {
                SmlScheme = smlScheme;
                SmpServerDomainName = smpServerDomainName;
            }

            public string SmlScheme { get; }
            public string SmpServerDomainName { get;  }

            /// <summary>
            /// Creates a <see cref="ESensConfig"/> configuration data object from a set of given <paramref name="properties"/>.
            /// </summary>
            /// <param name="properties">The custom defined properties.</param>
            /// <returns></returns>
            public static ESensConfig From(IDictionary<string, string> properties)
            {
                return new ESensConfig(
                    TrimDots(properties.ReadOptionalProperty("SmlScheme", "iso6523-actorid-upis")),
                    TrimDots(properties.ReadOptionalProperty("SmpServerDomainName", "isaitb.acc.edelivery.tech.ec.europa.eu")));
            }

            private static string TrimDots(string s) => s.Trim('.');
        }

        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="party"/> using a given <paramref name="properties"/>.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task<XmlDocument> RetrieveSmpMetaData(
            Party party, 
            IDictionary<string, string> properties)
        {
            if (party.PrimaryPartyId == null)
            {
                throw new InvalidOperationException("Given invalid 'ToParty'; requires a 'PartyId'");
            }

            var smpUrl = CreateSmpServerUrl(party, ESensConfig.From(properties));

            return await RetrieveSmpMetaData(smpUrl);
        }

        private static Uri CreateSmpServerUrl(Party party, ESensConfig config)
        {
            string hashedPartyId = CalculateMD5Hash(party.PrimaryPartyId);

            var host = $"b-{hashedPartyId}.{config.SmlScheme}.{config.SmpServerDomainName}";
            var path = $"{config.SmlScheme}::{party.PrimaryPartyId}/services/{DocumentIdentifierScheme}::{DocumentIdentifier}";

            var builder = new UriBuilder
            {
                Host = host,
                Path = path
            };

            return builder.Uri;
        }

        private static string CalculateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                byte[] hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                foreach (byte t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        private static async Task<XmlDocument> RetrieveSmpMetaData(Uri smpServerUri)
        {
            Logger.Info($"Contacting SMP server at {smpServerUri} to retrieve meta-data");

            HttpResponseMessage response = await HttpClient.GetAsync(smpServerUri);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpListenerException((int)response.StatusCode, "Unexpected result returned from SMP Service");
            }

            if (response.Content.Headers.ContentType.MediaType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) == -1)
            {
                throw new NotSupportedException($"An XML response was expected from the SMP server instead of {response.Content.Headers.ContentType.MediaType}");
            }

            var result = new XmlDocument();
            result.Load(await response.Content.ReadAsStreamAsync());

            return result;
        }

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="smpMetaData"></param>
        /// <returns></returns>
        public SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            CompleteMessageProperties(pmode, smpMetaData);

            CompleteCollaborationInfo(pmode, smpMetaData);

            CompleteSendConfiguration(pmode, smpMetaData);

            return pmode;
        }

        private static void CompleteMessageProperties(SendingProcessingMode sendingPMode, XmlDocument smpMetaData)
        {
            sendingPMode.MessagePackaging = sendingPMode.MessagePackaging ?? new SendMessagePackaging();
            sendingPMode.MessagePackaging.MessageProperties =
                sendingPMode.MessagePackaging.MessageProperties ?? new List<MessageProperty>();

            SetFinalRecipient(sendingPMode, smpMetaData);

            SetOriginalSender(sendingPMode);
        }

        private static void SetOriginalSender(SendingProcessingMode sendingPMode)
        {
            MessageProperty existingOriginalSender =
                sendingPMode.MessagePackaging.MessageProperties.FirstOrDefault(
                    p => p.Name.Equals("originalSender", StringComparison.OrdinalIgnoreCase));

            if (existingOriginalSender == null)
            {
                var originalSender = new MessageProperty { Name = "originalSender", Value = "urn:oasis:names:tc:ebcore:partyid-type:unregistered:C1" };
                sendingPMode.MessagePackaging.MessageProperties.Add(originalSender);
            }
        }

        private static void SetFinalRecipient(SendingProcessingMode sendingPMode, XmlDocument smpMetaData)
        {
            MessageProperty finalRecipient = GetFinalRecipient(smpMetaData);

            MessageProperty existingFinalRecipient =
                sendingPMode.MessagePackaging.MessageProperties.FirstOrDefault(
                    p => p.Name.Equals("finalRecipient", StringComparison.OrdinalIgnoreCase));

            if (existingFinalRecipient != null)
            {
                sendingPMode.MessagePackaging.MessageProperties.Remove(existingFinalRecipient);
            }

            sendingPMode.MessagePackaging.MessageProperties.Add(finalRecipient);
        }

        private static MessageProperty GetFinalRecipient(XmlNode smpMetaData)
        {
            XmlNode node = smpMetaData.SelectSingleNode("//*[local-name()='ParticipantIdentifier']");
            if (node == null) { throw new InvalidDataException("No ParticipantIdentifier element found in SMP meta-data"); }

            var finalRecipient = new MessageProperty { Name = "finalRecipient", Value = node.InnerText };

            XmlAttribute schemeAttribute = node.Attributes?
                .OfType<XmlAttribute>()
                .FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase));

            if (schemeAttribute != null)
            {
                finalRecipient.Type = schemeAttribute.Value;
            }

            return finalRecipient;
        }
        
        private static void CompleteCollaborationInfo(SendingProcessingMode sendingPMode, XmlDocument smpMetaData)
        {
            if (sendingPMode.MessagePackaging.CollaborationInfo == null)
            {
                sendingPMode.MessagePackaging.CollaborationInfo = new Model.PMode.CollaborationInfo();
            }

            SetCollaborationService(sendingPMode, smpMetaData);

            SetCollaborationAction(sendingPMode, smpMetaData);
        }
        
        private static void SetCollaborationService(SendingProcessingMode sendingPMode, XmlNode smpMetaData)
        {
            XmlNode processIdentifier = 
                smpMetaData.SelectSingleNode("//*[local-name()='ProcessList']/*[local-name()='Process']/*[local-name()='ProcessIdentifier']");

            if (processIdentifier == null)
            {
                throw new ConfigurationErrorsException("Unable to complete CollaborationInfo: ProcessIdentifier element not found in SMP metadata");
            }

            string serviceValue = processIdentifier.InnerText;
            string serviceType = null;

            XmlAttribute schemeAttribute = processIdentifier.Attributes?
                .OfType<XmlAttribute>()
                .FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase));

            if (schemeAttribute != null)
            {
                serviceType = schemeAttribute.Value;
            }

            sendingPMode.MessagePackaging.CollaborationInfo.Service = new Service
            {
                Value = serviceValue,
                Type = serviceType
            };
        }

        private static void SetCollaborationAction(SendingProcessingMode sendingPMode, XmlNode smpMetaData)
        {
            XmlNode documentIdentifier =
                smpMetaData.SelectSingleNode("//*[local-name()='ServiceInformation']/*[local-name()='DocumentIdentifier']");

            if (documentIdentifier == null)
            {
                throw new ConfigurationErrorsException("Unable to complete CollaborationInfo: DocumentIdentifier element not found in SMP metadata");
            }

            sendingPMode.MessagePackaging.CollaborationInfo.Action = documentIdentifier.InnerText;
        }

        private static void CompleteSendConfiguration(SendingProcessingMode sendingPMode, XmlNode smpMetaData)
        {
            XmlNode endPoint = 
                smpMetaData.SelectSingleNode("//*[local-name()='ServiceEndpointList']/*[local-name()='Endpoint' and @transportProfile='bdxr-transport-ebms3-as4-v1p0']");

            if (endPoint == null)
            {
                throw new InvalidDataException("No ServiceEndpointList/Endpoint element found in SMP meta-data");
            }

            CompletePushConfiguration(sendingPMode, endPoint);

            AddCertificateInformation(sendingPMode, endPoint);
        }

        private static void CompletePushConfiguration(SendingProcessingMode sendingPMode, XmlNode endPoint)
        {
            XmlNode address = endPoint.SelectSingleNode("*[local-name()='EndpointReference']/*[local-name()='Address']");

            if (address == null)
            {
                throw new InvalidDataException("No ServiceEndpointList/Endpoint/EndpointReference/Address element found in SMP meta-data");
            }

            if (sendingPMode.PushConfiguration == null)
            {
                sendingPMode.PushConfiguration = new PushConfiguration();
            }

            sendingPMode.PushConfiguration.Protocol = new Protocol
            {
                Url = address.InnerText
            };
        }

        private static void AddCertificateInformation(SendingProcessingMode sendingPMode, XmlNode endPoint)
        {
            XmlNode certificateNode = endPoint.SelectSingleNode("*[local-name()='Certificate']");
            if (certificateNode == null) { return; }

            sendingPMode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate
            {
                Certificate = certificateNode.InnerText
            };
            sendingPMode.Security.Encryption.CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate;

            var cert = new X509Certificate2(Convert.FromBase64String(certificateNode.InnerText), (string) null);

            sendingPMode.MessagePackaging.PartyInfo = sendingPMode.MessagePackaging.PartyInfo ?? new PartyInfo();
            sendingPMode.MessagePackaging.PartyInfo.ToParty = new Party
            {
                Role = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/responder"
            };
            sendingPMode.MessagePackaging.PartyInfo.ToParty.PartyIds.Add(new PartyId
            {
                Id = cert.GetNameInfo(X509NameType.SimpleName, false),
                Type = "urn:oasis:names:tc:ebcore:partyid-type:unregistered"
            });
        }       
    }
}