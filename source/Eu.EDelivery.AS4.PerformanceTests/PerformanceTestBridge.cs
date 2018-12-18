﻿using System;
using System.Diagnostics;
using System.Threading;
using Eu.EDelivery.AS4.PerformanceTests.Fixture;
using Polly;
using Xunit;
using Xunit.Abstractions;

namespace Eu.EDelivery.AS4.PerformanceTests
{
    public enum CornerStartup { Auto = 1, Manual = 2 }

    /// <summary>
    /// Bridge to add an extra abstraction layer for the AS4 Corner creation/destruction.
    /// </summary>
    [Collection(CornersCollection.CollectionId)]
    public class PerformanceTestBridge : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly Stopwatch _stopWatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTestBridge"/> class.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        /// <param name="outputHelper"></param>
        /// <param name="startup"></param>
        public PerformanceTestBridge(
            CornersFixture fixture, 
            ITestOutputHelper outputHelper,
            CornerStartup startup = CornerStartup.Auto)
        {
            Corner2 = fixture.Corner2;
            Corner3 = fixture.Corner3;

            if (startup == CornerStartup.Auto)
            {
                Corner2.Start();
                Corner3.Start();
            }

            Corner2.TryCleanupMessages();
            Corner3.TryCleanupMessages();

            _outputHelper = outputHelper;
            _stopWatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets the facade for the AS4 Corner 2 instance.
        /// </summary>
        protected Corner Corner2 { get; }

        /// <summary>
        /// Gets the facade for the AS4 Corner 3 instance.
        /// </summary>
        protected Corner Corner3 { get; }

        /// <summary>
        /// Start polling for a single message on the delivered directory to assert using the <paramref name="assertion"/>
        /// </summary>
        /// <param name="corner">Corner to use as delivered target.</param>
        /// <param name="timeoutMin">Amount to retry when polling for payloads</param>
        /// <param name="assertion">Assertion of the delivered message.</param>
        protected void PollingTillFirstPayload(Corner corner, int timeoutMin, Action assertion)
        {
            Policy.Timeout(TimeSpan.FromMinutes(timeoutMin))
                  .Wrap(Policy.HandleResult<int>(deliveredCount => deliveredCount < 2)
                              .WaitAndRetryForever(_ => TimeSpan.FromSeconds(5)))
                  .Execute(() =>
                  {
                      int deliveredCount = corner.CountDeliveredMessages();
                      _outputHelper.WriteLine($"Poll while: (Actual Delivered: {deliveredCount}) == 2");
                      return deliveredCount;
                  });
        }

        /// <summary>
        /// Start the polling of messages on the delivered directory to assert using the <paramref name="assertion" /> till the
        /// <paramref name="messageCount" /> is reached.
        /// </summary>
        /// <param name="messageCount">Amount of messages to wait for.</param>
        /// <param name="pollingRetries"></param>
        /// <param name="corner">Corner to use as delivered target.</param>
        /// <param name="assertion">Assertion of delivered messages.</param>
        protected void PollingTillAllMessages(int messageCount, int pollingRetries, Corner corner, Action assertion)
        {
            PollingForMessages(
                predicate: () =>
                {
                    int deliveredCount = corner.CountDeliveredMessages(searchPattern: "*.xml");
                    _outputHelper.WriteLine($"Poll while: (Expected Delivered: {messageCount}) <= (Actual Delivered: {deliveredCount})");
                    return messageCount <= deliveredCount;
                }, 
                assertion: assertion, 
                range: new PollingRange(pollingRetries, retrySeconds: 10));
        }

        private static void PollingForMessages(Func<bool> predicate, Action assertion, PollingRange range)
        {
            while (range.InRange)
            {
                if (predicate())
                {
                    assertion();
                    return;
                }

                range.Increase();
                Thread.Sleep(range.RetryInterval);
            }

            // Assert anyway to let the test fail.
            assertion();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stopWatch.Stop();
            _outputHelper.WriteLine($"Performance Test took: {_stopWatch.Elapsed:g} to run");

            Corner2.Stop();
            Corner3.Stop();
        }  
    }

    public class PollingRange
    {
        private readonly int _polligRetries;
        private int _currentRetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PollingRange"/> class.
        /// </summary>
        /// <param name="polligRetries">The retry Count.</param>
        /// <param name="retrySeconds">The retry Seconds.</param>
        public PollingRange(int polligRetries, int retrySeconds)
        {
            _polligRetries = polligRetries;
            RetryInterval = TimeSpan.FromSeconds(retrySeconds);
        }

        /// <summary>
        /// Gets the retry interval.
        /// </summary>
        /// <value>
        /// The retry interval.
        /// </value>
        public TimeSpan RetryInterval { get; }

        /// <summary>
        /// Gets a value indicating whether [in range].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in range]; otherwise, <c>false</c>.
        /// </value>
        public bool InRange => _currentRetry <= _polligRetries;

        /// <summary>
        /// Increases this instance.
        /// </summary>
        public void Increase() => ++_currentRetry;
    }
}