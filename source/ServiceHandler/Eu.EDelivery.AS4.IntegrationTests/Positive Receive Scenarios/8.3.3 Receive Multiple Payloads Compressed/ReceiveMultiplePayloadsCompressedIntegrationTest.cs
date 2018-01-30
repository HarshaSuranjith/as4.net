﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._3_Receive_Multiple_Payloads_Compressed
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveMultiplePayloadsCompressedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsCompressedSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.3-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.3-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath, fileCount: 2, validation: ValidateOnAS4InputPath));
            Assert.True(PollingAt(Holodeck.HolodeckALocations.InputPath, validation: _ => Holodeck.AssertReceiptOnHolodeckA()));
        }

        protected void ValidateOnAS4InputPath(IEnumerable<FileInfo> files)
        {
            AssertPayloads(files.Where(f => f.Extension == ".jpg"));
            AssertXmlFiles(files.Where(f => f.Extension == ".xml"));
        }

        private static void AssertPayloads(IEnumerable<FileInfo> files)
        {
            var sendPayload = new FileInfo(Holodeck.HolodeckALocations.JpegPayloadPath);

            Assert.All(files, f => Assert.Equal(sendPayload.Length, f.Length));
        }

        private static void AssertXmlFiles(IEnumerable<FileInfo> files)
        {
            Assert.Equal(2, files.Count());
            Console.WriteLine($@"There're {files.Count()} incoming Xml Documents found");
        }
    }
}