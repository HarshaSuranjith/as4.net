﻿using System.Collections.Generic;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitMessagePropertiesResolver"/>
    /// </summary>
    public class GivenSubmitMessagePropertiesResolverFacts
    {
        public class GivenValidArguments : GivenSubmitMessagePropertiesResolverFacts
        {
            [Fact]
            public void ThenResolveSubmitMessagePropertiesFromSubmitMessage()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreateaPopulatedSendingPMode();
                // Act
                MessageProperty[] properties = new SubmitMessagePropertiesResolver().Resolve(submitMessage);
                // Assert
                Assert.Equal(1, properties.Length);
            }

            [Fact]
            public void ThenResolverSubmitMessagePropertiesFromSubmitMessageAndPMode()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                SendingProcessingMode pmode = base.CreateaPopulatedSendingPMode();
                MessageProperty pmodeProperty = CreatePopulatedMessageProperty();
                pmode.MessagePackaging.MessageProperties = new List<MessageProperty> { pmodeProperty };
                submitMessage.PMode = pmode;
                // Act
                MessageProperty[] properties = new SubmitMessagePropertiesResolver().Resolve(submitMessage);
                // Assert
                Assert.Equal(2, properties.Length);
            }

            private MessageProperty CreatePopulatedMessageProperty()
            {
                return new MessageProperty
                {
                    Name = "pmode-name",
                    Type = "pmode-type",
                    Value = "pmode-value"
                };
            }
        }

        protected SubmitMessage CreatePopulatedSubmitMessage()
        {
            return AS4XmlSerializer.Deserialize<SubmitMessage>(Properties.Resources.submitmessage);
        }

        protected SendingProcessingMode CreateaPopulatedSendingPMode()
        {
            return AS4XmlSerializer.Deserialize<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
        }
    }
}