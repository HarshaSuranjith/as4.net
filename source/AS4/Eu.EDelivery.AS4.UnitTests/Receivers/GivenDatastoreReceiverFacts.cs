﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="DatastoreReceiver" />
    /// </summary>
    public class GivenDatastoreReceiverFacts : GivenDatastoreFacts
    {
        private readonly DatastoreReceiver _receiver;
        private SendingProcessingMode _pmode;

        public GivenDatastoreReceiverFacts()
        {
            this._receiver = new DatastoreReceiver(
                storeExpression: () => new DatastoreContext(base.Options),
                findExpression: x => x.OutMessages.Where(m => m.Operation == Operation.ToBeSent),
                updatedOperation: Operation.Sending);

            SeedDataStoreWithOutMessage();
        }

        private void SeedDataStoreWithOutMessage()
        {
            // Insert the seed data that is expected by all test methods
            using (var context = new DatastoreContext(base.Options))
            {
                context.OutMessages.Add(CreateStubOutMessage());
                context.SaveChanges();
            }
        }

        private OutMessage CreateStubOutMessage()
        {
            this._pmode = new SendingProcessingMode();
            return new OutMessage
            {
                Operation = Operation.ToBeSent,
                PMode = AS4XmlSerializer.ToString(this._pmode),
                MessageBody = new byte[0]
            };
        }

        /// <summary>
        /// Testing if the receiver succeeds
        /// </summary>
        public class GivenOutDatastoreReceiverSucceeds : GivenDatastoreReceiverFacts
        {
            [Fact]
            public void ThenConfigureSucceeds()
            {
                // Arrange
                IDictionary<string, string> properties = CreateDefaultDatastoreReceiverProperties();
                // Act
                this._receiver.Configure(properties);
                // Assert
                Assert.Same(properties, properties);
            }

            private IDictionary<string, string> CreateDefaultDatastoreReceiverProperties()
            {
                return new ConcurrentDictionary<string, string>()
                {
                    ["Table"] = "OutMessages",
                    ["Field"] = "Operation",
                    ["Value"] = "ToBeSend",
                    ["Update"] = "Sending",
                };
            }

            [Fact]
            public void ThenStartReceivingSucceeds()
            {
                // Arrange
                var source = new CancellationTokenSource();
                // Act
                base._receiver.StartReceiving(
                    (message, token) => AssertOnReceivedMessage(message, source), source.Token);
            }

            private Task<InternalMessage> AssertOnReceivedMessage(
                ReceivedMessage message, CancellationTokenSource source)
            {
                // Assert
                Assert.NotNull(message);
                Assert.IsType<ReceivedMessageEntityMessage>(message);
                Assert.NotNull(message.RequestStream);

                source.Cancel();
                return Task.FromResult(NullInternalMessage.Instance);
            }
        }
    }
}