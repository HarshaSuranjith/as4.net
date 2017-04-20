﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._11_Send_Single_Payload_with_Message_Ids
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SinglePayloadMessageIdsIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.1.11-sample.xml";

        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;
        private readonly Holodeck _holodeck;

        public SinglePayloadMessageIdsIntegrationTest()
        {
            _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
            _as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";

            _holodeck = new Holodeck();
        }

        [Fact]
        public void ThenSendingSinglePayloadWithMessageIdsSucceeds()
        {
            // Before
            CleanUpFiles(HolodeckBInputPath);
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            CopyPModeToHolodeckB("8.1.11-pmode.xml");
            File.Copy(_as4MessagesPath, _as4OutputPath);
            ReplaceTokenInFile("__MESSAGEID__", GenerateId(), _as4OutputPath);
            ReplaceTokenInFile("__REFTOMESSAGEID__", GenerateId(), _as4OutputPath);

            // Act
            AS4Component.Start();

            // Assert
            bool areFilesFound = PollingAt(AS4ReceiptsPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Single Payload with Message Properties Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Send Single Payload with Message Id failed");
        }

        private static string GenerateId() => Guid.NewGuid().ToString("N");

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            AssertPayloads();
            AssertAS4Receipt();
        }

        private void AssertPayloads()
        {
            IEnumerable<FileInfo> files = new DirectoryInfo(HolodeckBInputPath).GetFiles();

            _holodeck.AssertEarthPayload(files.FirstOrDefault(f => f.Extension.Equals(".jpg")));

            FileInfo receipt = files.FirstOrDefault(f => f.Extension.Equals(".xml"));
            if (receipt != null)
            {
                AssertHolodeckReceipt(receipt);
            }
        }

        private static void AssertHolodeckReceipt(FileSystemInfo receipt)
        {
            var xmlDocument = new XmlDocument();
            if (receipt != null)
            {
                xmlDocument.Load(receipt.FullName);
            }

            AssertXmlTag("RefToMessageId", xmlDocument);
            AssertXmlTag("MessageId", xmlDocument);
            AssertXmlTag("ConversationId", xmlDocument);
        }

        private static void AssertXmlTag(string localName, XmlNode xmlDocument)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode($"//*[local-name()='{localName}']");

            Assert.NotNull(xmlNode);
            Console.WriteLine($@"{localName} found in Receipt");
        }

        private static void AssertAS4Receipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}