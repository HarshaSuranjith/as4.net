﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="NotifyUpdateDatastoreStep" />
    /// </summary>
    public class GivenNotifyUpdateDatastoreStepForOutMessageFacts : GivenDatastoreFacts
    {
        private readonly NotifyUpdateDatastoreStep _step;

        public GivenNotifyUpdateDatastoreStepForOutMessageFacts()
        {
            _step = new NotifyUpdateDatastoreStep();
        }

        public class GivenValidArguments : GivenNotifyUpdateDatastoreStepForOutMessageFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithValidNotifyMessageAsync(string sharedId)
            {
                // Arrange
                InsertDefaultOutMessage(sharedId);
                NotifyMessageEnvelope notifyMessage = CreateNotifyMessage(sharedId);
                var internalMessage = new MessagingContext(notifyMessage);

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertOutMessage(
                    notifyMessage.MessageInfo.MessageId,
                    m =>
                    {
                        Assert.Equal(Operation.Notified, OperationUtils.Parse(m.Operation));
                        Assert.Equal(OutStatus.Notified, OutStatusUtils.Parse(m.Status));
                    });
            }

            private void InsertDefaultOutMessage(string sharedId)
            {
                var outMessage = new OutMessage(sharedId);
                
                outMessage.SetStatus(OutStatus.Ack);
                outMessage.SetOperation(Operation.Notifying);

                GetDataStoreContext.InsertOutMessage(outMessage);
            }

            private static NotifyMessageEnvelope CreateNotifyMessage(string id)
            {
                var msgInfo = new MessageInfo { MessageId = id };

                return new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, string.Empty, typeof(OutMessage));
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (var context = GetDataStoreContext())
                {
                    OutMessage outMessage = context.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(outMessage);
                }
            }
        }
    }
}