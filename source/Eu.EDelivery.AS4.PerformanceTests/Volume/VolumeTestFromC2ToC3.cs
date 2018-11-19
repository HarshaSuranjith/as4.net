﻿using System;
using System.Diagnostics;
using Eu.EDelivery.AS4.PerformanceTests.Fixture;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Eu.EDelivery.AS4.PerformanceTests.Properties.Resources;

namespace Eu.EDelivery.AS4.PerformanceTests.Volume
{
    /// <summary>
    /// 1. C2 (product A) to C3 (product B) oneWay signed and encrypted with increasing number of messages of a 10KB payload.
    /// </summary>
    public class VolumeTestFromC2ToC3 : PerformanceTestBridge
    {
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTestFromC2ToC3" /> class.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        /// <param name="outputHelper">The console output for the test run.</param>
        public VolumeTestFromC2ToC3(
            CornersFixture fixture, 
            ITestOutputHelper outputHelper) 
                : base(fixture, outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void Submit_100_Messages_Result_In_100_Delivered_UserMessages_And_100_Notified_Receipts()
        {
            // Arrange
            const int messageCount = 100;

            // Act
            Corner2.PlaceMessages(messageCount, SIMPLE_ONEWAY_TO_C3);

            // Assert
            PollingTillAllMessages(
                messageCount, 
                pollingRetries: 120, 
                corner: Corner3, 
                assertion: () => AssertMessages(messageCount));

            Assert.True(
                messageCount == Corner2.CountReceivedReceipts(), 
                $"Corner 2 notifies {messageCount} receipts");
        }

        private void AssertMessages(int messageCount)
        {
            AssertOnFileCount(messageCount, "*.jpg", $"Payloads count expected to be '{messageCount}'");
            AssertOnFileCount(messageCount, "*.xml", $"Deliver Message count expected to be '{messageCount}'");
        }

        private void AssertOnFileCount(int expectedCount, string searchPattern, string userMessage)
        {
            int actualCount = Corner3.CountDeliveredMessages(searchPattern);

            if (expectedCount != actualCount)
            {
                throw new AssertActualExpectedException(expectedCount, actualCount, userMessage);
            }
        }
    }
}
