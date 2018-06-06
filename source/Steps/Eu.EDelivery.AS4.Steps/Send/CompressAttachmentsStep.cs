﻿using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the attachments of an AS4 message must be compressed.
    /// </summary>
    [Info("Compress AS4 Message attachments if necessary")]
    [Description("This step compresses the attachments of an AS4 Message if compression is enabled in the sending PMode.")]
    public class CompressAttachmentsStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Compress the <see cref="AS4Message" /> if required
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (!messagingContext.SendingPMode.MessagePackaging.UseAS4Compression)
            {
                return ReturnSameMessagingContext(messagingContext);
            }

            CompressAS4Message(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static StepResult ReturnSameMessagingContext(MessagingContext messagingContext)
        {
            Logger.Debug(
                $"{messagingContext.LogTag} No compression will happen because the" + 
                $" Sending PMode {messagingContext.SendingPMode.Id} compression is disabled");

            return StepResult.Success(messagingContext);
        }

        private static void CompressAS4Message(MessagingContext context)
        {
            try
            {
                Logger.Info(
                    $"{context.LogTag} Compress AS4 Message attachments with GZip compression");

                context.AS4Message.CompressAttachments();
            }
            catch (SystemException exception)
            {
                throw ThrowAS4CompressingException(exception);
            }
        }

        private static Exception ThrowAS4CompressingException(Exception innerException)
        {
            const string description = "(Receive) Attachments cannot be compressed because of an exception";
            Logger.Error(description);

            return new InvalidDataException(description, innerException);
        }
    }
}