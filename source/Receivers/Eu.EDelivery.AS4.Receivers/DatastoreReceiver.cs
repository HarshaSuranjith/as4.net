﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver to poll the database to get the messages which validates a given Expression
    /// </summary>
    [Info("Datastore receiver")]
    public class DatastoreReceiver : PollingTemplate<Entity, ReceivedMessage>, IReceiver
    {
        private readonly Func<DatastoreContext> _storeExpression;

        private DatastoreReceiverSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver" /> class
        /// </summary>
        public DatastoreReceiver() : this(() => new DatastoreContext(Config.Instance)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver" /> class.
        /// </summary>
        /// <param name="storeExpression"></param>
        public DatastoreReceiver(Func<DatastoreContext> storeExpression)
        {
            _storeExpression = storeExpression;
        }

        protected override ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start Receiving on the Data Store
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            if (_settings == null)
            {
                throw new InvalidOperationException("The DatastoreReceiver is not configured.");
            }

            LogReceiverSpecs(true);
            StartPolling(messageCallback, cancellationToken);
        }

        public void StopReceiving()
        {
            LogReceiverSpecs(false);
        }

        private void LogReceiverSpecs(bool startReceiving)
        {
            string action = startReceiving ? "Start" : "Stop";
            Logger.Debug($"{action} Receiving on Datastore {_settings.DisplayString}");
        }

        private IEnumerable<Entity> GetMessagesEntitiesForConfiguredExpression()
        {
            // Use a TransactionScope to get the highest TransactionIsolation level.
            IEnumerable<Entity> entities = Enumerable.Empty<Entity>();

            using (DatastoreContext context = _storeExpression())
            {
                IDbContextTransaction transaction = context.Database.BeginTransaction();

                try
                {
                    entities = FindAnyMessageEntitiesWithConfiguredExpression(context);
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    LogExceptionAndInner(exception);
                    transaction.Rollback();
                }
            }

            return entities;
        }

        // ReSharper disable once InconsistentNaming
        private PropertyInfo __tableSetPropertyInfo;

        private PropertyInfo GetTableSetPropertyInfo()
        {
            if (__tableSetPropertyInfo == null)
            {
                __tableSetPropertyInfo = typeof(DatastoreContext).GetProperty(_settings.TableName);
            }

            return __tableSetPropertyInfo;
        }

        private IEnumerable<Entity> FindAnyMessageEntitiesWithConfiguredExpression(DatastoreContext context)
        {
            // TODO: refactor this using Dynamic Linq; pass the Filter expression to the Dynamic Linq Where extension method.            
            var tablePropertyInfo = GetTableSetPropertyInfo();

            // TODO: exception handling; extract method and keep trakc of it 

            var dbSet = tablePropertyInfo.GetValue(context) as IQueryable<Entity>;

            if (dbSet == null)
            {
                throw new ConfigurationErrorsException($"The configured table {_settings.TableName} could not be found");
            }

            // TODO: 
            // - validate the Filter clause for sql injection
            // - make sure that single quotes are used around string vars.  (Maybe make it dependent on the DB type, same is true for escape characters [] in sql server, ...             

            IEnumerable<Entity> entities;

            string filterExpression = Filter.Replace("\"", "\'");

            try
            {
                entities =
                    dbSet.FromSql($"SELECT * FROM {this.Table} WHERE {filterExpression}")
                         .OrderBy(x => x.InsertionTime)
                         .Take(this.TakeRows)
                         .ToList();
            }
            catch
            {
                // The InMemory database does not support the FromSql clause.
                // In this case, fall back on the Dynamic Linq option.
                // As soon as we have a solution for the NotMapped properties, this solution 
                // should replace the FromSql solution as well.
                filterExpression = Filter.Replace("\'", "\"");

                entities = dbSet.Where(filterExpression).OrderBy(x => x.InsertionTime).Take(this.TakeRows).ToList();
            }

            if (!entities.Any())
            {
                return entities;
            }

            LockEntitiesBeforeContinueToProcessThem(entities);

            context.SaveChanges();

            return entities;
        }

        // ReSharper disable once InconsistentNaming (__ indicates that this field should not be used directly)
        private PropertyInfo __updateFieldProperty;

        private PropertyInfo GetUpdateFieldProperty(Entity entity)
        {
            if (__updateFieldProperty == null)
            {
                __updateFieldProperty = entity.GetType().GetProperty(_settings.UpdateField);
            }

            return __updateFieldProperty;
        }

        private void LockEntitiesBeforeContinueToProcessThem(IEnumerable<Entity> entities)
        {
            if (entities.Any() == false)
            {
                return;
            }

            PropertyInfo property = GetUpdateFieldProperty(entities.First());

            foreach (Entity entity in entities)
            {
                object updateValue = Conversion.Convert(property.PropertyType, this.Update);

                property.SetValue(entity, updateValue);
            }
        }

        private async void ReceiveMessageEntity(
            MessageEntity messageEntity,
            Function messageCallback,
            CancellationToken token)
        {
            Logger.Info($"Received Message from Datastore with Ebms Message Id: {messageEntity.EbmsMessageId}");

            using (Stream stream = await messageEntity.RetrieveMessageBody(Registry.Instance.MessageBodyStore))
            {
                if (stream == null)
                {
                    Logger.Error($"MessageBody cannot be retrieved for Ebms Message Id: {messageEntity.EbmsMessageId}");
                }
                else
                {
                    ReceivedMessage receivedMessage = CreateReceivedMessage(messageEntity, stream);
                    try
                    {
                        await messageCallback(receivedMessage, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        receivedMessage.UnderlyingStream.Dispose();
                    }
                }
            }
        }

        private static ReceivedMessage CreateReceivedMessage(MessageEntity messageEntity, Stream stream)
        {
            return new ReceivedMessageEntityMessage(messageEntity, stream, messageEntity.ContentType);
        }

        private static async void ReceiveEntity(Entity entity, Function messageCallback, CancellationToken token)
        {
            var message = new ReceivedEntityMessage(entity);
            MessagingContext result = await messageCallback(message, token).ConfigureAwait(false);
            result?.Dispose();
        }

        private void LogExceptionAndInner(Exception exception)
        {
            Logger.Error(exception.Message);
            Logger.Trace(exception.StackTrace);

            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException.Message);
                Logger.Trace(exception.InnerException.StackTrace);
            }
        }

        #region Configuration

        [Info("Table", required: true)]
        private string Table => _settings.TableName;

        [Info("Filter", required: true)]
        private string Filter => _settings.Filter;

        [Info("How many rows to take", defaultValue: SettingKeys.TakeRowsDefault, type: "int32")]
        private int TakeRows => _settings.TakeRows;

        [Info("Update", attributes: new[] { "field" })]
        private string Update => _settings.UpdateValue;

        private static class SettingKeys
        {
            public const string PollingInterval = "PollingInterval";
            public const string Table = "Table";
            public const string Filter = "Filter";
            public const string TakeRows = "BatchSize";
            public const string TakeRowsDefault = "20";
            public const string Update = "Update";
            public const string PollingIntervalDefault = "00:00:03";
        }

        [Info("Polling interval (every)", defaultValue: SettingKeys.PollingIntervalDefault)]
        protected override TimeSpan PollingInterval => _settings.PollingInterval;

        private static TimeSpan GetPollingIntervalFromProperties(IDictionary<string, Setting> properties)
        {
            if (properties.ContainsKey(SettingKeys.PollingInterval) == false)
            {
                return DatastoreReceiverSettings.DefaultPollingInterval;
            }

            var pollingInterval = properties[SettingKeys.PollingInterval];
            return pollingInterval.Value.AsTimeSpan(DatastoreReceiverSettings.DefaultPollingInterval);
        }

        public void Configure(DatastoreReceiverSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        void IReceiver.Configure(IEnumerable<Setting> settings)
        {
            var properties = settings.ToDictionary(s => s.Key, s => s);

            var configuredTakeRecords = properties.ReadOptionalProperty(SettingKeys.TakeRows, null);

            if (configuredTakeRecords == null || Int32.TryParse(configuredTakeRecords.Value, out var takeRecords) == false)
            {
                takeRecords = DatastoreReceiverSettings.DefaultTakeRows;
            }

            var pollingInterval = GetPollingIntervalFromProperties(properties);

            var updateSetting = properties.ReadMandatoryProperty(SettingKeys.Update);

            if (updateSetting["field"] == null)
            {
                throw new ConfigurationErrorsException("The Update setting does not contain a field attribute that indicates the field that must be updated.");
            }

            _settings = new DatastoreReceiverSettings(properties.ReadMandatoryProperty(SettingKeys.Table).Value,
                                                      properties.ReadMandatoryProperty(SettingKeys.Filter).Value,
                                                      updateSetting["field"].Value,
                                                      updateSetting.Value,
                                                      pollingInterval,
                                                      takeRecords);
        }

        #endregion

        /// <summary>
        /// Get the Out Messages from the Store with <see cref="Operation.ToBeSent" /> as Operation
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<Entity> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            try
            {
                return GetMessagesEntitiesForConfiguredExpression();
            }
            catch (Exception exception)
            {
                Logger logger = LogManager.GetCurrentClassLogger();

                logger.Error($"An error occured while polling the datastore: {exception.Message}");
                logger.Error(
                    $"Polling on table {Table} with interval {PollingInterval.TotalSeconds} seconds.");
                logger.Error(exception.StackTrace);

                return Enumerable.Empty<Entity>();
            }
        }

        /// <summary>
        /// Describe what to do when a Out Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback"></param>
        /// <param name="token"></param>
        protected override void MessageReceived(Entity entity, Function messageCallback, CancellationToken token)
        {
            var messageEntity = entity as MessageEntity;

            if (messageEntity != null)
            {
                ReceiveMessageEntity(messageEntity, messageCallback, token);
            }
            else
            {
                ReceiveEntity(entity, messageCallback, token);
            }
        }

        protected override void HandleMessageException(Entity message, Exception exception)
        {
            Logger.Error(exception.Message);
            var aggregate = exception as AggregateException;

            if (aggregate == null)
            {
                return;
            }

            foreach (Exception ex in aggregate.InnerExceptions)
            {
                LogExceptionAndInner(ex);
            }
        }

        protected override void ReleasePendingItems()
        {
            // TODO: we should release the records that have been held locked by this
            // DataStoreReceiver so that they won't be locked forever.
            // -> Reset the records that have been locked by this process and who'sestatus is still the same.
        }
    }
}