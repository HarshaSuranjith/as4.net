﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    [NotConfigurable]
    [ExcludeFromCodeCoverage]
    public class MinderTestCreateDeliverEnvelopeStep : IConfigStep
    {
        private string _uriPrefix;

        /// <summary>
        /// Configure the step with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            _uriPrefix = properties.ReadMandatoryProperty("Uri");
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            bool includeAttachments = true;

            var collaborationInfo = messagingContext.ReceivingPMode.MessagePackaging?.CollaborationInfo;

            if (collaborationInfo != null &&
                (collaborationInfo.Action?.Equals("ACT_SIMPLE_ONEWAY_SIZE", StringComparison.OrdinalIgnoreCase) ?? false) &&
                (collaborationInfo.Service?.Value?.Equals("SRV_SIMPLE_ONEWAY_SIZE", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                includeAttachments = false;
            }

            DeliverMessageEnvelope deliverMessage = CreateDeliverMessageEnvelope(messagingContext, includeAttachments);
            messagingContext.ModifyContext(deliverMessage);

            return StepResult.SuccessAsync(messagingContext);
        }

        private DeliverMessageEnvelope CreateDeliverMessageEnvelope(MessagingContext context, bool includeAttachments)
        {
            AS4Message as4Message = context.AS4Message;
            UserMessage deliverMessage = CreateMinderDeliverMessage(as4Message);

            // The Minder Deliver Message should be an AS4-Message.
            AS4Message msg = AS4Message.Create(deliverMessage);

            if (includeAttachments)
            {
                foreach (Attachment attachment in as4Message.Attachments)
                {
                    msg.AddAttachment(attachment);
                }
            }

            byte[] content = SerializeAS4Message(msg);

            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo
                {
                    MessageId = deliverMessage.MessageId,
                    RefToMessageId = deliverMessage.RefToMessageId
                },
                deliverMessage: content,
                contentType: msg.ContentType);
        }

        private static byte[] SerializeAS4Message(AS4Message msg)
        {
            ISerializer serializer = AS4.Common.Registry.Instance.SerializerProvider.Get(msg.ContentType);

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(msg, memoryStream, CancellationToken.None);
                return memoryStream.ToArray();
            }
        }

        private UserMessage CreateMinderDeliverMessage(AS4Message as4Message)
        {
            UserMessage userMessage = as4Message.FirstUserMessage;

            var deliverMessage = new UserMessage(userMessage.MessageId)
            {
                RefToMessageId = userMessage.RefToMessageId,
                Timestamp = userMessage.Timestamp,
                CollaborationInfo =
                {
                    Action = "Deliver",
                    Service = {Value = _uriPrefix},
                    ConversationId = userMessage.CollaborationInfo.ConversationId
                },
                Sender = new Party($"{_uriPrefix}/sut", userMessage.Receiver.PartyIds.FirstOrDefault()),
                Receiver = new Party($"{_uriPrefix}/testdriver", new PartyId("minder"))
            };

            // Party Information: sender is the receiver of the AS4Message that has been received.
            //                    receiver is minder.

            // Set the PayloadInfo.
            foreach (PartInfo partInfo in userMessage.PayloadInfo)
            {
                deliverMessage.AddPartInfo(partInfo);
            }

            deliverMessage.AddMessageProperty(new MessageProperty("MessageId", userMessage.MessageId));
            AddMessageProperty(deliverMessage, "RefToMessageId", userMessage.RefToMessageId);
            AddMessageProperty(deliverMessage, "ConversationId", userMessage.CollaborationInfo.ConversationId);

            AddMessageProperty(deliverMessage, "Service", userMessage.CollaborationInfo.Service.Value);
            AddMessageProperty(deliverMessage, "Action", userMessage.CollaborationInfo.Action);
            AddMessageProperty(deliverMessage, "ConversationId", userMessage.CollaborationInfo.ConversationId);

            AddMessageProperty(deliverMessage, "FromPartyId", userMessage.Sender.PartyIds.First().Id);
            AddMessageProperty(deliverMessage, "FromPartyRole", userMessage.Sender.Role);

            AddMessageProperty(deliverMessage, "ToPartyId", userMessage.Receiver.PartyIds.First().Id);
            AddMessageProperty(deliverMessage, "ToPartyRole", userMessage.Receiver.Role);

            AddMessageProperty(deliverMessage, userMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals("originalSender")));
            AddMessageProperty(deliverMessage, userMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals("finalRecipient")));

            return deliverMessage;
        }

        private static void AddMessageProperty(UserMessage message, string propertyName, string propertyValue)
        {
            if (propertyValue == null)
            {
                return;
            }

            AddMessageProperty(message, new MessageProperty(propertyName, propertyValue));
        }

        private static void AddMessageProperty(UserMessage message, MessageProperty property)
        {
            if (property == null)
            {
                return;
            }

            message.AddMessageProperty(property);
        }
    }
}
