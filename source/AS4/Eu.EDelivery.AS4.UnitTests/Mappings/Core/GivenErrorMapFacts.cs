﻿using AutoMapper;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using System;
using Eu.EDelivery.AS4.Mappings.Core;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="ErrorMap"/>
    /// </summary>
    public class GivenErrorMapFacts
    {
        public GivenErrorMapFacts()
        {            
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenErrorMapFacts
        {
            [Fact]
            public void ThenMessageIdIsCorrectlyMapped()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                Xml.SignalMessage signalMessage = GetPopulatedXmlError();
                signalMessage.MessageInfo.MessageId = messageId;

                // Act
                var error = Mapper.Map<AS4.Model.Core.Error>(signalMessage);

                // Assert
                Assert.Equal(messageId, error.MessageId);
            }

            [Fact]
            public void ThenErrorDescriptionIsCorreclyMapped()
            {
                // Arrange
                string descriptionValue = Guid.NewGuid().ToString();
                string languageValue = Guid.NewGuid().ToString();

                Xml.SignalMessage signalMessage = GetPopulatedXmlError();
                signalMessage.Error[0].Description.Value = descriptionValue;
                signalMessage.Error[0].Description.lang = languageValue;

                // Act
                var error = Mapper.Map<AS4.Model.Core.Error>(signalMessage);

                // Assert
                Assert.Equal(descriptionValue, error.Errors[0].Description.Value);
                Assert.Equal(languageValue, error.Errors[0].Description.Language);
            }

        }

        protected Xml.SignalMessage GetPopulatedXmlError()
        {
            return new Eu.EDelivery.AS4.Xml.SignalMessage()
            {
                MessageInfo = new Eu.EDelivery.AS4.Xml.MessageInfo()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                },
                Error = new[] {
                    new Eu.EDelivery.AS4.Xml.Error
                    {
                        category = "myCategory",
                        Description = new Description { lang = "en", Value = "this is a long description" },
                        errorCode = "errorCode",
                        ErrorDetail = "errorDetail",
                        origin = "origin",
                        refToMessageInError = "refToMessageInError",
                        severity = "severity",
                        shortDescription = "shortDescription"
                    }
                }
            };
        }
    }
}