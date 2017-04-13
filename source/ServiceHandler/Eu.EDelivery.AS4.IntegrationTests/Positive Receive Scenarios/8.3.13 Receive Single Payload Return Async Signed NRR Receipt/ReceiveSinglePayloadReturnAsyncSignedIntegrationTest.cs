﻿using System;
using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._13_Receive_Single_Payload_Return_Async_Signed_NRR_Receipt
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveSinglePayloadReturnAsyncSignedIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.13-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveSinglePayloadReturnAsyncSignedIntegrationTest()
        {
            this._holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            this._destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            this._holodeck = new Holodeck();
        }

        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.StartAS4Component();
            base.CleanUpFiles(AS4FullInputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            base.CopyPModeToHolodeckA("8.3.13-pmode.xml");

            // Act
            File.Copy(this._holodeckMessagesPath, this._destFileName);

            // Assert
            bool areFilesFound = AreFilesFound();
            if (areFilesFound) Console.WriteLine(@"Receive Single Payload Return Async Signed NRR Integration Test succeeded!");
            else Retry();
        }

        private void Retry()
        {
            var startDir = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = startDir.GetFiles("*.jpg", SearchOption.AllDirectories);
            Console.WriteLine($@"Polling failed, retry to check for the files. {files.Length} Files are found");

            ValidatePolledFiles(files);
        }

        private bool AreFilesFound()
        {
            const int retryCount = 2000;
            return base.PollingAt(Properties.Resources.holodeck_A_input_path, "*.xml", retryCount);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            this._holodeck.AssertReceiptOnHolodeckA();
        }
    }
}
