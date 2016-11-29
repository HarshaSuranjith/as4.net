﻿using Eu.EDelivery.AS4.Fe.Services;
using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public interface IRuntimeLoader : IModular
    {
        IEnumerable<ItemType> Receivers { get; }
        IEnumerable<ItemType> Steps { get; }
        IEnumerable<ItemType> Transformers { get; }
        IRuntimeLoader Initialize();
    }
}