﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Outgoing Message Data Entity Schema
    /// </summary>
    public class OutMessage : MessageEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessage"/> class.
        /// </summary>
        internal OutMessage()
        {
            // Internal ctor to prevent that instances are created directly.
            // TODO: perhaps this class should not have a default ctor, but a ctor
            // which takes an AS4Message parameter ...  (Interferes with the Fe Unittests atm).
        }
        
        public void SetStatus(OutStatus status)
        {
            Status = status.ToString();
        }

    }
}