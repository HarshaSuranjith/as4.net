﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class SqliteDbCommand : IAS4DbCommand
    {
        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SqliteDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        public async Task CreateDatabase()
        {
            await _context.Database.MigrateAsync();
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
            if (!DatastoreTable.IsTableNameKnown(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            _context.Database.ExecuteSqlCommand("BEGIN EXCLUSIVE");
            string filterExpression = filter.Replace("\'", "\"");

            return DatastoreTable.FromTableName(tableName)(_context)
                .Where(filterExpression)
                .OrderBy(x => x.InsertionTime)
                .Take(takeRows)
                .ToList();
        }

        /// <summary>
        /// Delete the Messages Entities that are inserted passed a given <paramref name="retentionPeriod"/> 
        /// and has a <see cref="Operation"/> within the given <paramref name="allowedOperations"/>.
        /// </summary>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        public void BatchDeleteMessagesOverRetentionPeriod(TimeSpan retentionPeriod, IEnumerable<Operation> allowedOperations)
        {
            foreach (string table in DatastoreTable.MessageTables)
            {
                string command =
                    $"DELETE FROM {table} " +
                    $"WHERE InsertionTime<datetime('now', '-{retentionPeriod.TotalDays} day') " +
                    $"AND Operation IN({string.Join(", ", allowedOperations.Select(x => "'" + x.ToString() + "'"))})";

                _context.Database.ExecuteSqlCommand(command);

                LogManager.GetCurrentClassLogger().Debug($"Done cleaning '{table}'");
            }
        }
    }
}
