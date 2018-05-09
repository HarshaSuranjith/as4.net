﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    public class ReceiveMessageTransformer : ITransformer
    {
        private readonly IConfig _config;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private IDictionary<string, string> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveMessageTransformer"/> class.
        /// </summary>
        public ReceiveMessageTransformer() : this (Config.Instance) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveMessageTransformer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ReceiveMessageTransformer(IConfig configuration)
        {
            _config = configuration;
        }

        public const string ReceivingPModeKey = "ReceivingPMode";

        [Info("Receiving Processing Mode", required: false)]
        [Description("Receiving Processing Mode identifier to indicate which default Processing Mode should be used during the transformation of the incoming message.")]
        private string ReceivingPMode => _properties?.ReadOptionalProperty(ReceivingPModeKey);

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties)
        {
            _properties = properties;
        }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (message.UnderlyingStream == null)
            {
                throw new InvalidMessageException(
                    "The incoming stream is not an ebMS Message. " + 
                    "Only ebMS messages conform with the AS4 Profile are supported.");
            }

            if (!ContentTypeSupporter.IsContentTypeSupported(message.ContentType))
            {
                throw new InvalidMessageException(
                    $"ContentType is not supported {message.ContentType}{Environment.NewLine}" +
                    $"Supported ContentTypes are {Constants.ContentTypes.Soap} and {Constants.ContentTypes.Mime}");
            }

            ReceivedMessage m = await EnsureIncomingStreamIsSeekable(message);
            var context = new MessagingContext(m, MessagingContextMode.Receive);

            if (ReceivingPMode != null)
            {
                ReceivingProcessingMode pmode = 
                    _config.GetReceivingPModes()
                           ?.FirstOrDefault(p => p.Id == ReceivingPMode);

                if (pmode != null)
                {
                    context.ReceivingPMode = pmode;
                }
                else
                {
                    Logger.Warn(
                        $"Receiving PMode with Id: {ReceivingPMode} was configured as default PMode, {Environment.NewLine}" +
                        $"but this PMode cannot be found in the configured receiving PModes. {Environment.NewLine}" +
                        @"Configured Receiving PModes are placed on the folder: '.\config\receive-pmodes\'.");
                }
            }

            return context;
        }

        private static async Task<ReceivedMessage> EnsureIncomingStreamIsSeekable(ReceivedMessage m)
        {
            if (m.UnderlyingStream.CanSeek)
            {
                return m;
            }

            VirtualStream str = 
                VirtualStream.Create(
                    expectedSize: m.UnderlyingStream.CanSeek
                        ? m.UnderlyingStream.Length
                        : VirtualStream.ThresholdMax,
                    forAsync: true);

            await m.UnderlyingStream.CopyToFastAsync(str);
            str.Position = 0;

            return new ReceivedMessage(str, m.ContentType);
        }
    }
}
