﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._6_Receive_Multiple_Payloads_Compressed_Signed
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveMultiplePayloadsCompressedSignedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsCompressedSignedSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.6-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.6-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath, fileCount: 3, validation: ValidateDelivery), "No DeliverMessage and Payloads found at AS4.NET Component");
            Assert.True(PollingAt(Holodeck.HolodeckALocations.InputPath), "No Receipt found at Holodeck A");
        }

        private void ValidateDelivery(IEnumerable<FileInfo> files)
        {
            Assert.All(files.Where(f => f.Extension == ".jpg"), f => Assert.Equal(f.Length, Holodeck.HolodeckAJpegPayload.Length));
            Assert.Equal(files.Count(f => f.Extension == ".xml"), 2);
        }
    }
}