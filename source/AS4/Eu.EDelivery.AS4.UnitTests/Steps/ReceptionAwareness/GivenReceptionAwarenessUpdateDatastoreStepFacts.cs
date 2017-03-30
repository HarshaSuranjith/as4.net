﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.ReceptionAwareness;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.ReceptionAwareness
{
    /// <summary>
    /// Testing <see cref="ReceptionAwarenessUpdateDatastoreStep" />
    /// </summary>
    public class GivenReceptionAwarenessUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        public GivenReceptionAwarenessUpdateDatastoreStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenReceptionAwarenessUpdateDatastoreStepFacts
        {
            [Fact]
            public async Task ThenMessageIsAlreadyAwnseredAsync()
            {
                // Arrange
                Entities.ReceptionAwareness awareness = CreateDefaultReceptionAwareness();
                ArrangeMessageIsAlreadyAwnsered(awareness.InternalMessageId);
                InsertReceptionAwareness(awareness);

                var internalMessage = new InternalMessage {ReceptionAwareness = awareness};
                var step = new ReceptionAwarenessUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.True(x.IsCompleted));
            }

            private void ArrangeMessageIsAlreadyAwnsered(string messageId)
            {
                using (var context = new DatastoreContext(Options))
                {
                    var inMessage = new InMessage {EbmsRefToMessageId = messageId};
                    context.InMessages.Add(inMessage);
                    context.SaveChanges();
                }
            }

            [Fact]
            public async Task ThenMessageIsUnawnseredAsync()
            {
                // Arrange
                Entities.ReceptionAwareness awareness = CreateDefaultReceptionAwareness();
                awareness.CurrentRetryCount = awareness.TotalRetryCount;
                InsertReceptionAwareness(awareness);
                InsertOutMessage(awareness.InternalMessageId);

                var internalMessage = new InternalMessage {ReceptionAwareness = awareness};
                var step = new ReceptionAwarenessUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertInMessage(awareness.InternalMessageId);
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.True(x.IsCompleted));
            }

            private void AssertInMessage(string messageId)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage = context.InMessages.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));

                    Assert.NotNull(inMessage);
                }
            }

            private void AssertReceptionAwareness(string messageId, Action<Entities.ReceptionAwareness> condition)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    Entities.ReceptionAwareness awareness =
                        context.ReceptionAwareness.FirstOrDefault(a => a.InternalMessageId.Equals(messageId));

                    Assert.NotNull(awareness);
                    condition(awareness);
                }
            }
        }

        protected void InsertReceptionAwareness(Entities.ReceptionAwareness receptionAwareness)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                context.ReceptionAwareness.Add(receptionAwareness);
                context.SaveChanges();
            }
        }

        protected void InsertOutMessage(string messageId)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                var pmode = new SendingProcessingMode();
                string pmodeString = AS4XmlSerializer.Serialize(pmode);
                var outMessage = new OutMessage {EbmsMessageId = messageId, PMode = pmodeString};

                context.OutMessages.Add(outMessage);
                context.SaveChanges();
            }
        }

        protected Entities.ReceptionAwareness CreateDefaultReceptionAwareness()
        {
            return new Entities.ReceptionAwareness
            {
                CurrentRetryCount = 0,
                IsCompleted = false,
                InternalMessageId = "message-id",
                LastSendTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                RetryInterval = "00:00:00",
                TotalRetryCount = 5
            };
        }
    }
}