﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AS4.ParserService.Infrastructure;
using AS4.ParserService.Models;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Streaming;

namespace AS4.ParserService.Services
{
    internal class DecodeService
    {
        public async Task<DecodeResult> Process(DecodeMessageInfo info)
        {
            // Shortcuts have been taken.
            // Theoretically, it is possible that we receive an AS4Message that contains multiple 
            // message-parts.  We should process each message-part seperately.

            using (var receivedStream = new MemoryStream(info.ReceivedMessage))
            {
                var as4Message = await RetrieveAS4Message(info.ContentType, receivedStream);

                if (as4Message == null)
                {
                    return DecodeResult.CreateForBadRequest();
                }

                if (as4Message.IsSignalMessage)
                {
                    return DecodeResult.CreateAccepted(as4Message.PrimarySignalMessage is Receipt ? EbmsMessageType.Receipt : EbmsMessageType.Error,
                                                       as4Message.GetPrimaryMessageId());
                }

                // Start Processing
                var receivingPMode = await AssembleReceivingPMode(info);
                var respondingPMode = await AssembleRespondingPMode(info);

                if (receivingPMode == null)
                {
                    return DecodeResult.CreateForBadRequest();
                }

                if (respondingPMode == null)
                {
                    return DecodeResult.CreateForBadRequest();
                }

                var context = CreateMessagingContext(as4Message, receivingPMode, respondingPMode);

                try
                {
                    var decodeResult = await PerformInboundProcessing(context);

                    return decodeResult;
                }
                finally
                {
                    context.Dispose();
                }
            }
        }

        private static MessagingContext CreateMessagingContext(AS4Message receivedAS4Message, ReceivingProcessingMode receivingPMode, SendingProcessingMode respondingPMode)
        {
            var context = new MessagingContext(receivedAS4Message, MessagingContextMode.Receive)
            {
                ReceivingPMode = receivingPMode,
                SendingPMode = respondingPMode
            };

            return context;
        }

        private static async Task<DecodeResult> PerformInboundProcessing(MessagingContext context)
        {
            var processingResult =
                await StepProcessor.ExecuteStepsAsync(context, StepRegistry.GetInboundProcessingConfiguration());

            if (processingResult.AS4Message.IsUserMessage)
            {
                try
                {
                    var deliverPayloads = RetrievePayloadsFromMessage(processingResult.AS4Message);
                    var receivedMessageId = processingResult.EbmsMessageId;

                    var receiptResult =
                        await StepProcessor.ExecuteStepsAsync(processingResult, StepRegistry.GetReceiptCreationConfiguration());

                    if (receiptResult.AS4Message == null)
                    {
                        if (receiptResult.Exception != null)
                        {
                            var errorResult = await CreateAS4ErrorForException(processingResult, receiptResult);

                            return DecodeResult.CreateWithError(Serializer.ToByteArray(errorResult.AS4Message),
                                                                receivedMessageId,
                                                                errorResult.AS4Message.GetPrimaryMessageId());
                        }
                    }

                    return DecodeResult.CreateWithReceipt(deliverPayloads.ToArray(),
                                                          Serializer.ToByteArray(receiptResult.AS4Message),
                                                          receivedMessageId,
                                                          receiptResult.AS4Message.GetPrimaryMessageId());
                }
                finally
                {
                    processingResult.Dispose();
                }
            }

            if (!(processingResult.AS4Message.PrimarySignalMessage is Error))
            {
                throw new InvalidProgramException("An AS4 Error Message was expected.");
            }

            // What we have now, must an error.
            return DecodeResult.CreateWithError(Serializer.ToByteArray(processingResult.AS4Message),
                                                processingResult.AS4Message.PrimarySignalMessage.RefToMessageId,
                                                processingResult.AS4Message.PrimarySignalMessage.MessageId);
        }

        private static async Task<MessagingContext> CreateAS4ErrorForException(MessagingContext processingResult, MessagingContext receiptResult)
        {
            var ctx = new MessagingContext(processingResult.AS4Message, MessagingContextMode.Receive);
            ctx.ErrorResult = new ErrorResult(receiptResult.Exception.Message, ErrorAlias.Other);

            var createError = new CreateAS4ErrorStep();
            var errorResult = await createError.ExecuteAsync(ctx);

            return errorResult.MessagingContext;
        }

        private static IEnumerable<PayloadInfo> RetrievePayloadsFromMessage(AS4Message message)
        {
            foreach (var attachment in message.Attachments)
            {
                yield return new PayloadInfo(attachment.Id, attachment.ContentType, attachment.Content.ToBytes());
            }
        }

        private static async Task<ReceivingProcessingMode> AssembleReceivingPMode(DecodeMessageInfo info)
        {
            var pmode = await Deserializer.ToReceivingPMode(info.ReceivingPMode);

            if (pmode == null)
            {
                return null;
            }

            if (info.DecryptionCertificate != null && info.DecryptionCertificate.Length > 0)
            {
                pmode.Security.Decryption.DecryptCertificateInformation = new PrivateKeyCertificate()
                {
                    Certificate = Convert.ToBase64String(info.DecryptionCertificate),
                    Password = info.DecryptionCertificatePassword
                };
            }

            return pmode;
        }

        private static async Task<SendingProcessingMode> AssembleRespondingPMode(DecodeMessageInfo info)
        {
            var pmode = await Deserializer.ToSendingPMode(info.RespondingPMode);

            if (pmode == null)
            {
                return null;
            }

            if (pmode.Security?.Signing?.IsEnabled ?? false)
            {
                pmode.Security.Signing.SigningCertificateInformation = new PrivateKeyCertificate
                {
                    Certificate = Convert.ToBase64String(info.SigningResponseCertificate ?? new byte[] { }),
                    Password = info.SigningResponseCertificatePassword
                };
            }

            return pmode;
        }

        private static async Task<AS4Message> RetrieveAS4Message(string contentType, Stream receivedStream)
        {
            try
            {
                var deserializer = SerializerProvider.Default.Get(contentType);
                return await deserializer.DeserializeAsync(receivedStream, contentType, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }
    }
}