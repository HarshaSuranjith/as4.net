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

        private static readonly IDictionary<string, Func<Entity, string>> GetOperationString = 
            new Dictionary<string, Func<Entity, string>>
            {
                ["OutMessages"] = e => (e as OutMessage)?.Operation,
                ["InMessages"] = e => (e as InMessage)?.Operation,
                ["OutExceptions"] = e => (e as OutException)?.Operation,
                ["InExceptions"] = e => (e as InException)?.Operation
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
                                          && allowedOperations.Contains(
                                              OperationUtils.Parse(
                                                  GetOperationString[tableName](x) ??
                                                  Operation.NotApplicable.ToString())));

            if (tableName.Equals("OutMessages"))
            {
                long[] outMessageIds = entities.ToArray().Cast<OutMessage>().Select(m => m.Id).ToArray();
                _context.ReceptionAwareness.RemoveRange(
                    _context.ReceptionAwareness.Where(r => outMessageIds.Contains(r.RefToOutMessageId)).ToArray());
            }

            _context.RemoveRange(entities);
            _context.SaveChanges();
        }
    }
}