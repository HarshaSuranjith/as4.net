﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Receipt : SignalMessage
    {
        public UserMessage UserMessage { get; set; }
        public NonRepudiationInformation NonRepudiationInformation { get; set; }
    }

    public class NonRepudiationInformation
    {
        public ICollection<MessagePartNRInformation> MessagePartNRInformation { get; set; }

        public NonRepudiationInformation()
        {
            this.MessagePartNRInformation = new Collection<MessagePartNRInformation>();
        }
    }

    public class MessagePartNRInformation
    {
        public Reference Reference { get; set; }
    }

    public class Reference
    {
        public ICollection<ReferenceTransform> Transforms { get; set; }
        public ReferenceDigestMethod DigestMethod { get; set; }
        public string DigestValue { get; set; }
        public string URI { get; set; }
    }

    public class ReferenceTransforms
    {
        public ReferenceTransform Transform { get; set; }
    }

    public class ReferenceTransform
    {
        public string Algorithm { get; set; }

        public ReferenceTransform(string algorithm)
        {
            this.Algorithm = algorithm;
        }
    }

    public class ReferenceDigestMethod
    {
        public string Algorithm { get; set; }

        public ReferenceDigestMethod() {}

        public ReferenceDigestMethod(string algorithm)
        {
            this.Algorithm = algorithm;
        }
    }
}