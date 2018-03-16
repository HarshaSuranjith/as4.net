﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [NotConfigurable]
    [ExcludeFromCodeCoverage]
    public class ConformanceTestingSubmitReceiveMessageTransformer : ITransformer
    {
        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            // We receive an AS4Message from Minder, we should convert it to a SubmitMessage if the action is submit.
            // In any other case, we should just return a MessagingContext which contains the as4Message.
            var receivedStream = VirtualStream.CreateVirtualStream();

            await message.UnderlyingStream.CopyToAsync(receivedStream);
            receivedStream.Position = 0;

            var receivedMessage = new ReceivedMessage(receivedStream, message.ContentType);

            try
            {
                var transformer = new AS4MessageTransformer();
                var messagingContext = await transformer.TransformAsync(receivedMessage, cancellationToken);

                if (messagingContext.AS4Message?.PrimaryUserMessage?.CollaborationInfo?.Action?.Equals("Submit", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    var as4Message =
                        TransformMinderSubmitToAS4Message(messagingContext.AS4Message.PrimaryUserMessage, messagingContext.AS4Message.Attachments);
                    messagingContext = new MessagingContext(as4Message, MessagingContextMode.Submit);

                    AssignPModeToContext(messagingContext);

                    return messagingContext;
                }
                else
                {
                    receivedStream.Position = 0;
                    return new MessagingContext(receivedMessage, MessagingContextMode.Receive);
                }
            }
            catch (Exception ex)
            {
                var l = NLog.LogManager.GetCurrentClassLogger();
                l.Error(ex.Message);
                l.Trace(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    l.Error(ex.InnerException.Message);
                }
                throw;
            }
        }

        private static AS4Message TransformMinderSubmitToAS4Message(UserMessage submitMessage, IEnumerable<Attachment> attachments)
        {
            var userMessage = new UserMessage(GetPropertyValue(submitMessage.MessageProperties, "MessageId"))
            {
                RefToMessageId = GetPropertyValue(submitMessage.MessageProperties, "RefToMessageId"),
                Timestamp = DateTimeOffset.Now
            };

            SetCollaborationInfoProperties(userMessage, submitMessage.MessageProperties);

            SetPartyInformation(userMessage, submitMessage);

            SetMessageProperties(userMessage, submitMessage.MessageProperties);

            AS4Message result = CreateAS4Message(userMessage, submitMessage.PayloadInfo, attachments);

            return result;
        }

        private static void SetCollaborationInfoProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.CollaborationInfo.ConversationId = GetPropertyValue(properties, "ConversationId");
            userMessage.CollaborationInfo.Service.Value = GetPropertyValue(properties, "Service");
            userMessage.CollaborationInfo.Action = GetPropertyValue(properties, "Action");

            // AgreementRef must not be present in the AS4Message for minder.
            userMessage.CollaborationInfo.AgreementReference = null;
        }

        private static void SetPartyInformation(UserMessage userMessage, UserMessage submitMessage)
        {
            userMessage.Sender.PartyIds.First().Id = GetPropertyValue(submitMessage.MessageProperties, "FromPartyId");
            userMessage.Sender.PartyIds.First().Type = submitMessage.Sender.PartyIds.First().Type;
            userMessage.Sender.Role = GetPropertyValue(submitMessage.MessageProperties, "FromPartyRole");

            userMessage.Receiver.PartyIds.First().Id = GetPropertyValue(submitMessage.MessageProperties, "ToPartyId");
            userMessage.Receiver.PartyIds.First().Type = submitMessage.Receiver.PartyIds.First().Type;
            userMessage.Receiver.Role = GetPropertyValue(submitMessage.MessageProperties, "ToPartyRole");
        }

        private static void SetMessageProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            string[] whiteList = { "originalSender", "finalRecipient", "trackingIdentifier" };

            userMessage.MessageProperties = properties.Where(p => whiteList.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                                                         .ToList();
        }

        private static AS4Message CreateAS4Message(UserMessage userMessage, IEnumerable<PartInfo> payloadInfo, IEnumerable<Attachment> attachments)
        {
            userMessage.PayloadInfo = new List<PartInfo>(payloadInfo);

            var result = AS4Message.Create(userMessage, null);

            foreach (var attachment in attachments)
            {
                result.AddAttachment(attachment);
            }

            return result;
        }

        private static void AssignPModeToContext(MessagingContext context)
        {
            AS4Message as4Message = context.AS4Message;

            // The PMode that must be used is defined in the CollaborationInfo.Service property.
            var pmode = Config.Instance.GetSendingPMode(as4Message.PrimaryUserMessage.CollaborationInfo.Action);
            context.SendingPMode = pmode;
        }
       
        private static string GetPropertyValue(IList<MessageProperty> properties, string propertyName)
        {
            return properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}
