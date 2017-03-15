﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Outgoing Message Data Entity Schema
    /// </summary>
    public class OutMessage : MessageEntity
    {
        [NotMapped]
        public OutStatus Status { get; set; }

        public override string StatusString
        {
            get
            {
                return Status.ToString();
            }
            set
            {
                Status = (OutStatus)Enum.Parse(typeof(OutStatus), value, true);
            }
        }
    }
}