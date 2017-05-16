using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="AS4.Entities.MessageEntity"/>
    /// </summary>
    public class ReceivedMessageEntityMessage : ReceivedMessage
    {
        public MessageEntity MessageEntity { get; }

        public ReceivedMessageEntityMessage(MessageEntity messageEntity)
        {
            this.MessageEntity = messageEntity;
        }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage"/>
        /// </summary>
        /// <param name="message"></param>
        public override void AssignPropertiesTo(AS4Message message)
        {
            base.AssignPropertiesTo(message);

            if (MessageEntity is InMessage)
            {
                message.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
            }
            else if (MessageEntity is OutMessage)
            {
                message.SendingPMode = GetPMode<SendingProcessingMode>();
            }
        }

        public T GetPMode<T>() where T : class
        {
            return AS4XmlSerializer.FromString<T>(this.MessageEntity.PMode);
        }
    }
}