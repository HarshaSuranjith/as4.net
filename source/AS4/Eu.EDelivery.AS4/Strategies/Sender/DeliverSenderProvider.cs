﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Class to provide <see cref="IDeliverSender"/> implementations
    /// based on a given condition
    /// </summary>
    public class DeliverSenderProvider : IDeliverSenderProvider
    {
        private readonly ICollection<DeliverSenderEntry> _senders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverSenderProvider"/> class. 
        /// Create a new <see cref="DeliverSenderProvider"/>
        /// to select the provide the right <see cref="IDeliverSender"/> implementation
        /// </summary>
        public DeliverSenderProvider()
        {
            this._senders = new Collection<DeliverSenderEntry>();
        }

        /// <summary>
        /// Accept a given <paramref name="sender"/> for a given <paramref name="condition"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sender"></param>
        public void Accept(Func<string, bool> condition, IDeliverSender sender)
        {
            this._senders.Add(new DeliverSenderEntry(condition, sender));
        }

        /// <summary>
        /// Get the right <see cref="IDeliverSender"/> implementation
        /// for a given <paramref name="operationMethod"/>
        /// </summary>
        /// <param name="operationMethod"></param>
        /// <returns></returns>
        public IDeliverSender GetDeliverSender(string operationMethod)
        {
            DeliverSenderEntry entry = this._senders.FirstOrDefault(s => s.Condition(operationMethod));

            if(entry?.Sender == null)
                throw new AS4Exception($"No Deliver Sender found for a given {operationMethod} Operation Method");

            return entry.Sender;
        }

        /// <summary>
        /// Value Object to define a entry for the <see cref="IDeliverSenderProvider"/>
        /// </summary>
        private class DeliverSenderEntry
        {
            public Func<string, bool> Condition { get; }
            public IDeliverSender Sender { get; }

            public DeliverSenderEntry(Func<string, bool> condition, IDeliverSender sender)
            {
                this.Condition = condition;
                this.Sender = sender;
            }
        }
    }

    /// <summary>
    /// Interface to define the <see cref="IDeliverSender"/> selection
    /// </summary>
    public interface IDeliverSenderProvider
    {
        void Accept(Func<string, bool> condition, IDeliverSender sender);
        IDeliverSender GetDeliverSender(string operationMethod);
    }
}
