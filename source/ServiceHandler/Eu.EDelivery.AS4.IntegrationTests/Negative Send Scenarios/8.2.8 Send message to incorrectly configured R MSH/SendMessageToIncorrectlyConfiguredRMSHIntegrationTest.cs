﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._8_Send_message_to_incorrectly_configured_R_MSH
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SendMessageToIncorrectlyConfiguredRMSHIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.8-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SendMessageToIncorrectlyConfiguredRMSHIntegrationTest()
        {
            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendMessageFailes()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ErrorsPath);

            // Act
            File.Copy( this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = AreErrorFilesFound();
            if (areFilesFound) Console.WriteLine(@"Send Message to Incorrectly Configured R-MSH Integration Test succeeded!");
        }

        private bool AreErrorFilesFound()
        {
            const int milisecondsRetryCount = 3000;
            return base.PollTo(AS4ErrorsPath, "*.xml", milisecondsRetryCount);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            FileInfo notifyErrorFile = files.FirstOrDefault(f => f.Extension.Equals(".xml"));
            if (notifyErrorFile == null) return;

            Console.WriteLine($@"Notify Error Message found at: {notifyErrorFile.FullName}");
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(notifyErrorFile.FullName);

            AssertNotifyDescription(xmlDocument);
            AssertNotifyStatus(xmlDocument);
        }

        private void AssertNotifyStatus(XmlDocument xmlDocument)
        {
            XmlNode statusNode = xmlDocument.SelectSingleNode("//*[local-name()='Status']");
            Assert.NotNull(statusNode);
            Assert.Equal("Error", statusNode.InnerText);

            Console.WriteLine($@"Notify Error Message Status = {statusNode.InnerText}");
        }

        private void AssertNotifyDescription(XmlDocument xmlDocument)
        {
            XmlNode errorDetailNode = xmlDocument.SelectSingleNode("//*[local-name()='ErrorDetail']");
            Assert.NotNull(errorDetailNode);
            
            Console.WriteLine($@"Notify Error Message Error Detail: {errorDetailNode.InnerText}");
        }
    }
}