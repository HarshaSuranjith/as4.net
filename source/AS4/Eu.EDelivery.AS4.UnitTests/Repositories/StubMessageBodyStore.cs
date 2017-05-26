﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal class StubMessageBodyStore : IAS4MessageBodyStore
    {
        internal static StubMessageBodyStore Default => new StubMessageBodyStore();

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        public Task<string> SaveAS4MessageAsync(string location, AS4Message message, CancellationToken cancellation)
        {
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public Task<Stream> LoadMessagesBody(string location)
        {
            return Task.FromResult(Stream.Null);
        }

        [Fact]
        public void UpdatesComplete()
        {
            Assert.True(Default.UpdateAS4MessageAsync(null, null, CancellationToken.None).IsCompleted);
        }

        [Fact]
        public void SaveComplete()
        {
            Assert.True(Default.SaveAS4MessageAsync(null, null, CancellationToken.None).IsCompleted);
        }

        [Fact]
        public async Task LoadsEmpty()
        {
            Assert.Equal(Stream.Null, await Default.LoadMessagesBody(null));
        }
    }
}
