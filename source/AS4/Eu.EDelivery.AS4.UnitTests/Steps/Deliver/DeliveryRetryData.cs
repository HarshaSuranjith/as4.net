﻿using System.Collections;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    public class DeliveryRetryData : IEnumerable<object[]>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[]
            {
                new RetryData(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    deliverResult: DeliverMessageResult.Success,
                    uploadResult: UploadResult.Success("", ""),
                    expectedCurrentRetryCount: 1,
                    expectedOperation: Operation.Delivered,
                    expectedStatus: InStatus.Delivered)
            },
            new object[]
            {
                new RetryData(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    deliverResult: DeliverMessageResult.Failure(anotherRetryIsNeeded: true),
                    uploadResult: UploadResult.Failure(needsAnotherRetry: true),
                    expectedCurrentRetryCount: 2,
                    expectedOperation: Operation.ToBeDelivered,
                    expectedStatus: InStatus.Received)
            },
            new object[]
            {
                new RetryData(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    deliverResult: DeliverMessageResult.Failure(anotherRetryIsNeeded: false),
                    uploadResult: UploadResult.Failure(needsAnotherRetry: false),
                    expectedCurrentRetryCount: 1,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            },
            new object[]
            {
                new RetryData(
                    currentRetryCount: 3,
                    maxRetryCount: 3,
                    deliverResult: DeliverMessageResult.Failure(anotherRetryIsNeeded: true),
                    uploadResult: UploadResult.Failure(needsAnotherRetry: true),
                    expectedCurrentRetryCount: 3,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            },
            new object[]
            {
                new RetryData(
                    currentRetryCount: 3,
                    maxRetryCount: 3,
                    deliverResult: DeliverMessageResult.Failure(anotherRetryIsNeeded: false),
                    uploadResult: UploadResult.Failure(needsAnotherRetry: false),
                    expectedCurrentRetryCount: 3,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            }
        };

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<object[]> GetEnumerator()
        {
            return Inputs.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RetryData
    {
        public RetryData(
            int currentRetryCount,
            int maxRetryCount,
            DeliverMessageResult deliverResult,
            UploadResult uploadResult,
            int expectedCurrentRetryCount,
            Operation expectedOperation,
            InStatus expectedStatus)
        {
            CurrentRetryCount = currentRetryCount;
            MaxRetryCount = maxRetryCount;
            DeliverResult = deliverResult;
            ExpectedCurrentRetryCount = expectedCurrentRetryCount;
            ExpectedOperation = expectedOperation;
            ExpectedStatus = expectedStatus;
            UploadResult = uploadResult;
        }

        public int CurrentRetryCount { get; }

        public int MaxRetryCount { get; }

        public DeliverMessageResult DeliverResult { get; }

        public UploadResult UploadResult { get; }

        public int ExpectedCurrentRetryCount { get; }

        public Operation ExpectedOperation { get; }

        public InStatus ExpectedStatus { get; }
    }
}