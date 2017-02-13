﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Template Method Polling Base Class to expose the Polling mechanism over the Receivers
    /// </summary>
    /// <typeparam name="TIn">Incoming Message Type from the Polling LocationParameter</typeparam>
    /// <typeparam name="TOut">Out coming Message Type when the Message is Received</typeparam>
    public abstract class PollingTemplate<TIn, TOut>
    {
        protected abstract ILogger Logger { get; }
        protected abstract TimeSpan PollingInterval { get; }

        /// <summary>
        /// Start Polling to the given Target
        /// </summary>
        /// <param name="onMessage">Message Callback after the Message is received</param>
        /// <param name="cancellationToken"></param>
        protected void StartPolling(Func<TOut, CancellationToken, Task<InternalMessage>> onMessage, CancellationToken cancellationToken)
        {
            if (this.PollingInterval.Ticks <= 0)
                throw new ApplicationException("PollingInterval should be greater than zero");

            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = new List<Task>();
                IEnumerable<TIn> messagesToPoll = GetMessagesToPoll(cancellationToken);
                tasks.AddRange(CreateTaskForEachMessage(messagesToPoll, onMessage, cancellationToken));

                TryPollOnTarget(tasks, messagesToPoll, cancellationToken);
            }            
        }

        /// <summary>
        /// Poll on a given Target
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="messagesToPoll"></param>
        private void TryPollOnTarget(List<Task> tasks, IEnumerable<TIn> messagesToPoll, CancellationToken cancellationToken)
        {
            try
            {
                PollOnTarget(tasks, cancellationToken);
            }
            catch (Exception exception)
            {
                foreach (TIn message in messagesToPoll)
                {
                    HandleMessageException(message, exception);
                }
            }
        }

        private void PollOnTarget(List<Task> tasks, CancellationToken cancellationToken)
        {
            int taskCount = tasks.Count;
            Task.WaitAll(tasks.ToArray());

            bool isThereWork = taskCount > 0;
            if (isThereWork == false)
            {
                // TODO: modify this to Task.Delay().Wait instead ?
                Thread.Sleep(this.PollingInterval);
               // Task.Delay(this.PollingInterval, cancellationToken).Wait(cancellationToken);
            }
        }

        /// <summary>
        /// Declaration to where the Message are and can be polled
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TIn> GetMessagesToPoll(CancellationToken cancellationToken);

        /// <summary>
        /// Describe what to do when a exception is thrown
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected abstract void HandleMessageException(TIn message, Exception exception);

        /// <summary>
        /// Declaration to the action that has to executed when a Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback">Message Callback after the Message is received</param>
        /// <param name="token"></param>
        protected abstract void MessageReceived(
            TIn entity,
            Func<TOut, CancellationToken, Task<InternalMessage>> messageCallback,
            CancellationToken token);

        private IEnumerable<Task> CreateTaskForEachMessage(
            IEnumerable<TIn> messagesToPoll,
            Func<TOut, CancellationToken, Task<InternalMessage>> messageCallback,
            CancellationToken cancellationToken)
        {
            return messagesToPoll.Select(
                message => Task.Run(() => MessageReceived(message, messageCallback, cancellationToken)));
        }
    }
}