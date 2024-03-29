﻿using Eu.EDelivery.AS4.Fe.Hash;

namespace Eu.EDelivery.AS4.Fe.Pmodes.Model
{
    public class BasePmode<TPmode>
    {
        public PmodeType Type { get; set; }
        public string Name { get; set; }
        public virtual TPmode Pmode { get; set; }
        public string Hash { get; set; }
    }
}