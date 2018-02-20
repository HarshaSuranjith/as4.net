﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    /// <summary>
    /// Abstraction to introduce custom commands/queries on the specific DBMS storage types.
    /// </summary>
    public interface IAS4DbCommand
    {
        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        Task CreateDatabase();

        /// <summary>
        /// Exclusively retrieves the entities for the different DBMS storage types.
        /// </summary>
        /// <param name="tableName">Name of the Db table.</param>
        /// <param name="filter">Order by this field.</param>
        /// <param name="takeRows">Take this amount of rows.</param>
        /// <returns></returns>
        IEnumerable<Entity> ExclusivelyRetrieveEntities(
            string tableName,
            string filter,
            int takeRows);

        /// <summary>
        /// Delete the Messages Entities that are inserted passed a given <paramref name="retentionPeriod"/> 
        /// and has a <see cref="Operation"/> within the given <paramref name="allowedOperations"/>.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        void BatchDeleteOverRetentionPeriod(
            string tableName,
            TimeSpan retentionPeriod,
            IEnumerable<Operation> allowedOperations);
    }
}
