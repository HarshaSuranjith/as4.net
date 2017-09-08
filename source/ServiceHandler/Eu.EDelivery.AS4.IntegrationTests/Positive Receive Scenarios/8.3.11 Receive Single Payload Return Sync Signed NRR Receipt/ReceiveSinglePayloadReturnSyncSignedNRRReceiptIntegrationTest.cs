﻿using System;
using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._11_Receive_Single_Payload_Return_Sync_Signed_NRR_Receipt
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveSinglePayloadReturnSyncSignedNRRReceiptIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.11-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveSinglePayloadReturnSyncSignedNRRReceiptIntegrationTest()
        {
            _holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            _destFileName = $"{Holodeck.HolodeckALocations.OutputPath}{HolodeckMessageFilename}";
            _holodeck = new Holodeck();
        }

        [Fact]
        public void ThenReceiveSinglePayloadSyncSignedNRRReceipt()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.11-pmode.xml");

            // Act
            File.Copy(_holodeckMessagesPath, _destFileName);

            // Assert
            bool areFilesFound = PollingAt(Holodeck.HolodeckALocations.InputPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Receive Single Payload Return Sync Signed NRR Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Receive Single Payload Sync Signed NRR Receipt fails");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            _holodeck.AssertReceiptOnHolodeckA();
        }
    }
}
