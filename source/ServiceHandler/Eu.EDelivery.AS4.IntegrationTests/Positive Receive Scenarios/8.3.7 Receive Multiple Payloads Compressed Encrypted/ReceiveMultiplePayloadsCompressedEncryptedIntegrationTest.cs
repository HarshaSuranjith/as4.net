﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._7_Receive_Multiple_Payloads_Compressed_Encrypted
{
    /// <summary>
    /// Testing the Applicaiton with multiple payloads compressed and encrypted
    /// </summary>
    public class ReceiveMultiplePayloadsCompressedEncryptedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsSignedSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.7-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.7-sample.mmd");

            // Assert
           Assert.True(PollingAt(AS4FullInputPath, fileCount: 3), "No DeliverMessage and payloads found on AS4.NET Component");
            Assert.True(PollingAt(Holodeck.HolodeckALocations.InputPath), "No Receipt found at Holodeck A");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Assert.All(files.Where(f => f.Extension == ".jpg"), f => Assert.Equal(f.Length, Holodeck.HolodeckAPayload.Length));
            Assert.Equal(files.Count(f => f.Extension == ".xml"), 2);
        }
    }
}
