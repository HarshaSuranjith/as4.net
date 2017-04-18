﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._10._Send_Single_Payload_with_Message_Properties
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SinglePayloadMessagePropertiesIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.1.10-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SinglePayloadMessagePropertiesIntegrationTest()
        {
            this._as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendingSinglePayloadWithMessagePropertiesSucceeds()
        {
            // Before
            CleanUpFiles(HolodeckBInputPath);
            StartAS4Component();
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            CopyPModeToHolodeckB("8.1.10-pmode.xml");

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            bool areFilesFound = PollingAt(AS4ReceiptsPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Single Payload with Message Properties Integration Test succeeded!");
            }
            Assert.True(areFilesFound, "Single Payload with Message Properties failed");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads();
            AssertHolodeckReceiptFile();
            AssertAS4Receipt();
        }

        private void AssertPayloads()
        {
            FileInfo receivedPayload = new DirectoryInfo(base.HolodeckBInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(Path.GetFullPath($".\\{Properties.Resources.submitmessage_single_payload_path}"));

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private void AssertHolodeckReceiptFile()
        {
            FileInfo receipt = new DirectoryInfo(base.HolodeckBInputPath)
                .GetFiles("*.xml").FirstOrDefault();

            var xmlDocument = new XmlDocument();
            if (receipt != null) xmlDocument.Load(receipt.FullName);

            XmlNode messagePropertyNode = xmlDocument.SelectSingleNode("//*[local-name()='MessageProperties']");
            if (messagePropertyNode == null) return;

            Assert.Equal(2, messagePropertyNode.ChildNodes.Count);
            Console.WriteLine(@"Two Message Properties found in sended Receipt");
        }

        private void AssertAS4Receipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}