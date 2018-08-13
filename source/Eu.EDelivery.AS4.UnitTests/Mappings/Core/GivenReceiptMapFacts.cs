﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using CoreReference = Eu.EDelivery.AS4.Model.Core.Reference;
using CoreReceipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using XmlInformation = Eu.EDelivery.AS4.Xml.MessagePartNRInformation;
using XmlReceipt = Eu.EDelivery.AS4.Xml.Receipt;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    public class GivenReceiptMapFacts
    {
        public class GivenValidArguments : GivenReceiptMapFacts
        {
            [Fact]
            public async void ThenMapModelToXmlReceiptSucceeds()
            {
                // Arrange
                AS4Message as4Message = await GetPopulatedModelReceipt();
                var coreReceipt = as4Message.FirstSignalMessage as CoreReceipt;

                // Act
                var xmlReceipt = AS4Mapper.Map<XmlReceipt>(coreReceipt);

                // Assert
                AssertReceiptReferences(coreReceipt, xmlReceipt);
            }

            private static void AssertReceiptReferences(CoreReceipt coreReceipt, XmlReceipt xmlReceipt)
            {
                XmlInformation[] xmlInformations = xmlReceipt.NonRepudiationInformation.MessagePartNRInformation;
                IEnumerable<CoreReference> coreReferences = coreReceipt.NonRepudiationInformation.MessagePartNRIReferences;

                foreach (XmlInformation partNRInformation in xmlInformations)
                {
                    var xmlReference = partNRInformation.Item as ReferenceType;

                    CoreReference coreReference = coreReferences.FirstOrDefault(i => i.URI.Equals(xmlReference?.URI));

                    byte[] coreReferenceDigestValue = coreReference?.DigestValue ?? new byte[0];
                    Assert.True(xmlReference?.DigestValue.SequenceEqual(coreReferenceDigestValue));
                    ReferenceDigestMethod coreReferenceDigestMethod = coreReference?.DigestMethod;
                    Assert.Equal(xmlReference?.DigestMethod.Algorithm, coreReferenceDigestMethod?.Algorithm);
                }
            }
        }

        protected async Task<AS4Message> GetPopulatedModelReceipt()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.receipt));
            var serializer = new SoapEnvelopeSerializer();

            return await serializer.DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);
        }
    }
}