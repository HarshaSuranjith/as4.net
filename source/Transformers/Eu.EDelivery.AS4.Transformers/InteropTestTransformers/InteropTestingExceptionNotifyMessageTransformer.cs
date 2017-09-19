﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Transformers.InteropTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class InteropTestingExceptionNotifyMessageTransformer : NotifyMessageTransformer
    {
        protected override async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            var notifyTransformer = new InteropTestingNotifyMessageTransformer();

            return await notifyTransformer.CreateNotifyMessageEnvelope(as4Message, receivedEntityType);
        }
    }
}
