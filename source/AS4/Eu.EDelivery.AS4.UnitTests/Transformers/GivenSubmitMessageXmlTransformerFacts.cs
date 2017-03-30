﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing the <see cref="SubmitMessageXmlTransformer" />
    /// </summary>
    public class GivenSubmitMessageXmlTransformerFacts
    {
        private readonly string _pmodeId;
        private readonly SubmitMessageXmlTransformer _transformer;

        public GivenSubmitMessageXmlTransformerFacts()
        {
            _transformer = new SubmitMessageXmlTransformer();
            _pmodeId = "01-pmode";
        }

        protected SendingProcessingMode GetStubProcessingMode()
        {
            return new SendingProcessingMode {Id = _pmodeId};
        }

        protected MemoryStream WriteSubmitMessageToStream(SubmitMessage submitMessage)
        {
            var memoryStream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(SubmitMessage));
            serializer.Serialize(memoryStream, submitMessage);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Testing if the Transformer succeeds
        /// for the "Execute" Method
        /// </summary>
        public class GivenValidArgumentsToTransform : GivenSubmitMessageXmlTransformerFacts
        {
            [Fact]
            public async Task ThenPModeIsNotPartOfTheSerializationAsync()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    Collaboration = {
                                       AgreementRef = {
                                                         PModeId = "pmode-id"
                                                      }
                                    },
                    PMode = GetStubProcessingMode()
                };

                using (MemoryStream memoryStream = WriteSubmitMessageToStream(submitMessage))
                {
                    var receivedmessage = new ReceivedMessage(memoryStream);

                    // Act
                    InternalMessage internalMessage = await _transformer.TransformAsync(
                                                          receivedmessage,
                                                          CancellationToken.None);

                    // Assert
                    Assert.NotNull(internalMessage);
                    Assert.Null(internalMessage.SubmitMessage.PMode);
                }
            }

            [Fact]
            public async Task ThenTransformSucceedsWithPModeIdAsync()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    Collaboration = new CollaborationInfo {AgreementRef = new Agreement {PModeId = _pmodeId}}
                };

                using (MemoryStream memoryStream = WriteSubmitMessageToStream(submitMessage))
                {
                    var receivedMessage = new ReceivedMessage(memoryStream);

                    // Act
                    InternalMessage internalMessage = await _transformer.TransformAsync(
                                                          receivedMessage,
                                                          CancellationToken.None);

                    // Assert
                    Assert.NotNull(internalMessage);
                    Assert.Equal(_pmodeId, internalMessage.SubmitMessage.Collaboration.AgreementRef.PModeId);
                }
            }
        }

        public class GivenInvalidArgumentsToTransform : GivenSubmitMessageXmlTransformerFacts
        {
            [Fact]
            public async void SubmitMessageWithoutPModeIdIsNotAccepted()
            {
                var submitMessage = new SubmitMessage
                {
                    Collaboration = new CollaborationInfo {AgreementRef = new Agreement {PModeId = string.Empty}}
                };

                using (MemoryStream memoryStream = WriteSubmitMessageToStream(submitMessage))
                {
                    var receivedMessage = new ReceivedMessage(memoryStream);

                    // Act
                    await Assert.ThrowsAsync<AS4Exception>(
                        () => _transformer.TransformAsync(receivedMessage, CancellationToken.None));
                }
            }
        }
    }
}