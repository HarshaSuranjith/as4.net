using System;
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Data Store Connection Test Setup
    /// </summary>
    [Collection("Tests that impact datastore")] // Tests that belong to the same collection do not run in parallel.
    public class GivenDatastoreFacts : IDisposable
    {
        private readonly IServiceProvider _serviceProvider =
            new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

        protected DbContextOptions<DatastoreContext> Options { get; }

        protected Func<DatastoreContext> GetDataStoreContext { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenDatastoreFacts"/> class. 
        /// </summary>
        public GivenDatastoreFacts()
        {
            Options = CreateNewContextOptions();
            GetDataStoreContext = () => new DatastoreContext(Options, StubConfig.Default);
        }

        private DbContextOptions<DatastoreContext> CreateNewContextOptions()
        {
            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            return new DbContextOptionsBuilder<DatastoreContext>()
                   .UseInMemoryDatabase(Guid.NewGuid().ToString())
                   .UseInternalServiceProvider(_serviceProvider)
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.AmbientTransactionWarning))
                   .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                   .Options;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Disposing();

            GetDataStoreContext = null;
        }

        protected virtual void Disposing() { }
    }
}