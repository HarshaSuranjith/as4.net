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
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using EntityReceptionAwareness = Eu.EDelivery.AS4.Entities.ReceptionAwareness;

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
                EntityReceptionAwareness awareness = InsertAlreadyAnsweredMessage();

                var internalMessage = new InternalMessage {ReceptionAwareness = awareness};
                var step = new ReceptionAwarenessUpdateDatastoreStep(StubMessageBodyPersister.Default);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.Equal(ReceptionStatus.Completed, x.Status));
            }

            private EntityReceptionAwareness InsertAlreadyAnsweredMessage()
            {
                EntityReceptionAwareness awareness = CreateDefaultReceptionAwareness();

                InsertReceptionAwareness(awareness);

                ArrangeMessageIsAlreadyAnswered(awareness.InternalMessageId);

                return awareness;
            }

            private void ArrangeMessageIsAlreadyAnswered(string messageId)
            {
                using (var context = new DatastoreContext(Options))
                {
                    var inMessage = new InMessage {EbmsMessageId = "message-id", EbmsRefToMessageId = messageId};
                    context.InMessages.Add(inMessage);
                    context.SaveChanges();
                }
            }

            [Fact]
            public async Task ThenMessageIsUnansweredAsync()
            {
                // Arrange
                EntityReceptionAwareness awareness = CreateDefaultReceptionAwareness();
                awareness.CurrentRetryCount = awareness.TotalRetryCount;
                InsertReceptionAwareness(awareness);
                InsertOutMessage(awareness.InternalMessageId);

                var internalMessage = new InternalMessage {ReceptionAwareness = awareness};
                var step = new ReceptionAwarenessUpdateDatastoreStep(StubMessageBodyPersister.Default);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertInMessage(awareness.InternalMessageId);
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.Equal(ReceptionStatus.Completed, x.Status));
            }

            [Fact]
            public async Task ThenStatusIsResetToPending()
            {
                // Arrange
                EntityReceptionAwareness awareness = CreateDefaultReceptionAwareness();
                awareness.CurrentRetryCount = 0;
                // When the DataReceiver receives a ReceptionAwareness item, it's status is Locked to Busy.
                awareness.Status = ReceptionStatus.Busy; 

                InsertReceptionAwareness(awareness);
                InsertOutMessage(awareness.InternalMessageId);

                var internalMessage = new InternalMessage { ReceptionAwareness = awareness };
                var step = new ReceptionAwarenessUpdateDatastoreStep(StubMessageBodyPersister.Default);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.Equal(ReceptionStatus.Pending, x.Status));
            }

            private void AssertInMessage(string messageId)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage = context.InMessages.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));

                    Assert.NotNull(inMessage);
                }
            }

            private void AssertReceptionAwareness(string messageId, Action<EntityReceptionAwareness> condition)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    EntityReceptionAwareness awareness =
                        context.ReceptionAwareness.FirstOrDefault(a => a.InternalMessageId.Equals(messageId));

                    Assert.NotNull(awareness);
                    condition(awareness);
                }
            }
        }

        protected void InsertReceptionAwareness(EntityReceptionAwareness receptionAwareness)
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
                string pmodeString = AS4XmlSerializer.ToString(pmode);
                var outMessage = new OutMessage {EbmsMessageId = messageId, PMode = pmodeString};

                context.OutMessages.Add(outMessage);
                context.SaveChanges();
            }
        }

        protected EntityReceptionAwareness CreateDefaultReceptionAwareness()
        {
            return new EntityReceptionAwareness
            {
                CurrentRetryCount = 0,
                Status = ReceptionStatus.Pending,
                InternalMessageId = "message-id",
                LastSendTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                RetryInterval = "00:00:00",
                TotalRetryCount = 5
            };
        }
    }
}