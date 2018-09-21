﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class InMemoryDbCommand : IAS4DbCommand
    {
        private readonly DatastoreContext _context;

        private static readonly IDictionary<string, Func<Entity, Operation>> GetOperation = 
            new Dictionary<string, Func<Entity, Operation>>
            {
                ["OutMessages"] = e => ((OutMessage)e).Operation,
                ["InMessages"] = e => ((InMessage)e).Operation,
                ["OutExceptions"] = e => ((OutException)e).Operation,
                ["InExceptions"] = e => ((InException)e).Operation
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public InMemoryDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the exclusive lock isolation for the transaction of retrieval of entities.
        /// </summary>
        /// <value>The exclusive lock isolation.</value>
        public IsolationLevel? ExclusiveLockIsolation => new IsolationLevel();

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        public async Task CreateDatabase()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Exclusively retrieves the entities.
        /// </summary>
        /// <param name="tableName">Name of the Db table.</param>
        /// <param name="filter">Order by this field.</param>
        /// <param name="takeRows">Take this amount of rows.</param>
        /// <returns></returns>
        public IEnumerable<Entity> ExclusivelyRetrieveEntities(string tableName, string filter, int takeRows)
        {
            string filterExpression = filter.Replace("\'", "\"");

            return DatastoreTable.FromTableName(tableName)(_context)
                .Where(filterExpression)
                .ToList();
        }

        /// <summary>
        /// Delete the Messages Entities that are inserted passed a given <paramref name="retentionPeriod"/> 
        /// and has a <see cref="Operation"/> within the given <paramref name="allowedOperations"/>.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        public void BatchDeleteOverRetentionPeriod(
            string tableName,
            TimeSpan retentionPeriod,
            IEnumerable<Operation> allowedOperations)
        {
            IQueryable<Entity> entities =
                DatastoreTable.FromTableName(tableName)(_context)
                              .Cast<Entity>()
                              .Where(x => x.InsertionTime < DateTimeOffset.Now.Subtract(retentionPeriod)
                                          && allowedOperations.Contains(GetOperation[tableName](x)));

            _context.RemoveRange(entities);
            _context.SaveChanges();
        }

        /// <summary>
        /// Selects in a reliable way the ToBePiggyBacked SignalMessages stored in the OutMessage table.
        /// </summary>
        /// <param name="url">The endpoint to which the OutMessage SignalMessage should be Piggy Backed.</param>
        /// <param name="mpc">The MPC of the incoming PullRequest to match on the related UserMessage of the Piggy Backed SignalMessage.</param>
        /// <returns></returns>
        public IEnumerable<OutMessage> SelectToBePiggyBackedSignalMessages(string url, string mpc)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (mpc == null)
            {
                throw new ArgumentNullException(nameof(mpc));
            }

            // TODO: the 'Take' of the query should be configurable.
            return (from outM in _context.OutMessages
                    join inM in _context.InMessages
                        on outM.EbmsRefToMessageId
                        equals inM.EbmsMessageId
                    where outM.Operation == Operation.ToBePiggyBacked
                          && inM.Mpc == mpc
                          && outM.Url == url
                          && outM.EbmsMessageType != MessageType.UserMessage
                    select outM)
                   .OrderByDescending(m => m.InsertionTime)
                   .Take(10)
                   .AsEnumerable<OutMessage>();
        }
    }
}