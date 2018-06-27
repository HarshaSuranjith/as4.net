﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class UserMessage : MessageUnit
    {
        private readonly ICollection<PartInfo> _partInfos;
        private readonly ICollection<MessageProperty> _messageProperties;

        public string Mpc { get; set; }

        public Party Sender { get; set; }

        public Party Receiver { get; set; }

        public CollaborationInfo CollaborationInfo { get; set; }

        public IEnumerable<MessageProperty> MessageProperties => _messageProperties.AsEnumerable();

        public IEnumerable<PartInfo> PayloadInfo => _partInfos.AsEnumerable();

        // TODO: this should happen together with the adding of Attachments
        // TODO: only unique PartInfo's are allowed
        public void AddPartInfo(PartInfo p)
        {
            _partInfos.Add(p);
        }

        public void AddMessageProperty(MessageProperty p)
        {
            _messageProperties.Add(p);
        }

        [XmlIgnore]
        public bool IsDuplicate { get; set; }

        [XmlIgnore]
        public bool IsTest { get; set; }

        public bool ShouldSerializePayloadInfo()
        {
            // When this method returns false, the XmlSerializer will not write an XmlElement for the PayloadInfo attribute.
            return _partInfos.Any();
        }

        public UserMessage() : this(IdentifierFactory.Instance.Create())
        {

        }

        public UserMessage(string messageId) : base(messageId)
        {
            Sender = new Party(new PartyId { Id = Constants.Namespaces.EbmsDefaultFrom }) { Role = Constants.Namespaces.EbmsDefaultFrom };
            Receiver = new Party(new PartyId { Id = Constants.Namespaces.EbmsDefaultTo }) { Role = Constants.Namespaces.EbmsDefaultTo };
            CollaborationInfo = CollaborationInfo.Default;

            _partInfos = new Collection<PartInfo>();
            _messageProperties = new Collection<MessageProperty>();
        }


        public override string ToString()
        {
            return $"UserMessage [${this.MessageId}]";
        }
    }
}