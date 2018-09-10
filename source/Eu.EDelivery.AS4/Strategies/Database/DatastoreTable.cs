﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    /// <summary>
    /// Validate the name of the Datastore Table.
    /// </summary>
    internal static class DatastoreTable
    {
        public static readonly IDictionary<string, Func<DatastoreContext, IQueryable<Entity>>> TablesByName =
            new Dictionary<string, Func<DatastoreContext, IQueryable<Entity>>>
            {
                {"InMessages", c => c.InMessages},
                {"OutMessages", c => c.OutMessages},
                {"InExceptions", c => c.InExceptions},
                {"OutExceptions", c => c.OutExceptions},
                {"RetryReliability", c => c.RetryReliability}
            };

        /// <summary>
        /// Gets the message tables.
        /// </summary>
        /// <value>The message tables.</value>
        public static IEnumerable<string> DomainEntityTables =
            TablesByName.Keys.Where(k => !new[] { "ReceptionAwareness", "RetryReliability" }.Contains(k));

        /// <summary>
        /// Determines whether [is table name known] [the specified table name].
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>
        ///   <c>true</c> if [is table name known] [the specified table name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTableNameKnown(string tableName)
        {
            return TablesByName.ContainsKey(tableName);
        }

        /// <summary>
        /// Ensure that the given <paramref name="tableName"/> is known.
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <exception cref="ConfigurationErrorsException">Throws when the given <paramref name="tableName"/> isn't known.</exception>
        public static void EnsureTableNameIsKnown(string tableName)
        {
            if (!IsTableNameKnown(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }
        }

        /// <summary>
        /// Retrieve a selector function based on a given <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">The name of the table to determine the datastore selector</param>
        /// <returns></returns>
        /// <exception cref="ConfigurationErrorsException">Throws if the given <paramref name="tableName"/> isn't known</exception>
        public static Func<DatastoreContext, IQueryable<Entity>> FromTableName(string tableName)
        {
            if (!TablesByName.ContainsKey(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            return TablesByName[tableName];
        }
    }
}
