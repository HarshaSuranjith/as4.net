﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._8._Send_Multiple_Payloads_Compressed_Signed_Encrypted
{
    /// <summary>
    /// Testing the Application with multiple Payloads compressed, signed and encrypted
    /// </summary>
    public class MultiplePayloadsCompressedSignedEncryptedIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.1.8-sample.xml";
        private readonly string _as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";
        private readonly string _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";

        [Fact]
        public void ThenSendingMultiplePayloadCompressedEncryptedSucceeds()
        {
            // Before
            CleanUpFiles(HolodeckBInputPath);
            StartAS4Component();
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            CopyPModeToHolodeckB("8.1.8-pmode.xml");

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            bool areFilesFound = PollingAt(AS4ReceiptsPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Multiple Payloads Compressed, Signed and Encrypted Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Multiple Payloads Compressed, Signed and Encrypted failed");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads();
            AssertReceipt();
        }

        private void AssertPayloads()
        {
            FileInfo[] receivedPayloads = new DirectoryInfo(HolodeckBInputPath).GetFiles();

            var sentEarth = new FileInfo($".{Properties.Resources.submitmessage_single_payload_path}");
            var sentXml = new FileInfo($".{Properties.Resources.submitmessage_second_payload_path}");

            // Earth attachment
            FileInfo receivedEarth = receivedPayloads.SingleOrDefault(x => x.Extension == ".jpg");
            FileInfo receivedXml = receivedPayloads.SingleOrDefault(x => x.Name.Contains("sample"));

            Assert.NotNull(receivedEarth);
            Assert.NotNull(receivedXml);

            Assert.Equal(sentEarth.Length, receivedEarth.Length);
            Assert.Equal(sentXml.Length, receivedXml.Length);
        }

        private static void AssertReceipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}
