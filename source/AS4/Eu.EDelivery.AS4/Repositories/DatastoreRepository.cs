﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using NLog;

namespace Eu.EDelivery.AS4.Repositories
{
    /// <summary>
    /// High level repository to use the Data store in a uniform way
    /// </summary>
    public class DatastoreRepository : IDatastoreRepository
    {
        private readonly DatastoreContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreRepository"/> class. 
        /// Create a high level Repository with a given Data store Context
        /// </summary>
        /// <param name="datastore">
        /// </param>     
        public DatastoreRepository(DatastoreContext datastore)
        {
            _dbContext = datastore;
        }

        /// <summary>
        /// Get a <see cref="InMessage"/>
        /// for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public InMessage GetInMessageById(string messageId)
        {
            return _dbContext.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
        }

        public InMessage GetInMessage(Func<InMessage, bool> predicate)
        {
            return _dbContext.InMessages.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Get a <see cref="OutMessage"/> for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public OutMessage GetOutMessageById(string messageId)
        {
            return _dbContext.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
        }

        public IEnumerable<OutMessage> GetOutMessagesById(IEnumerable<string> messageIds)
        {
            return _dbContext.OutMessages.Where(m => messageIds.Contains(m.EbmsMessageId)).ToArray();
        }

        /// <summary>
        /// Get a <see cref="ReceptionAwareness"/>
        /// for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public ReceptionAwareness GetReceptionAwareness(string messageId)
        {
            return _dbContext.ReceptionAwareness.FirstOrDefault(a => a.InternalMessageId.Equals(messageId));
        }

        public IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds)
        {
            return _dbContext.ReceptionAwareness.Where(r => messageIds.Contains(r.InternalMessageId)).ToArray();
        }

        /// <summary>
        /// Insert a given <see cref="OutMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="outMessage"></param>
        public async Task InsertOutMessageAsync(OutMessage outMessage)
        {
            _dbContext.OutMessages.Add(outMessage);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Insert a given <see cref="OutException"/>
        /// into the Data store
        /// </summary>
        /// <param name="outException"></param>
        /// <returns></returns>
        public async Task InsertOutExceptionAsync(OutException outException)
        {
            _dbContext.OutExceptions.Add(outException);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Insert a given <see cref="InMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="inMessage"></param>
        /// <returns></returns>
        public async Task InsertInMessageAsync(InMessage inMessage)
        {
            _dbContext.InMessages.Add(inMessage);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Insert a given <see cref="InException"/>
        /// into the Data store
        /// </summary>
        /// <param name="inException"></param>
        /// <returns></returns>
        public async Task InsertInExceptionAsync(InException inException)
        {
            _dbContext.InExceptions.Add(inException);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Insert a given <see cref="ReceptionAwareness"/>
        /// into the Data store
        /// </summary>
        /// <param name="receptionAwareness"></param>
        /// <returns></returns>
        public async Task InsertReceptionAwarenessAsync(ReceptionAwareness receptionAwareness)
        {
            _dbContext.ReceptionAwareness.Add(receptionAwareness);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Update a found OutMessage (by AS4 Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateOutMessageAsync(string messageId, Action<OutMessage> updateAction)
        {
            OutMessage outMessage = _dbContext.OutMessages
                .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));

            if (outMessage == null)
            {
                LogManager.GetCurrentClassLogger().Error($"No OutMessage found for MessageId {messageId}");
                return;
            }
            updateAction(outMessage);
            outMessage.ModificationTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();

            // FRGH: code below replaces code above, perf opt.

            //////OutMessage msg = new OutMessage() {EbmsMessageId = messageId};
            //////context.Attach(msg);

            //////updateAction(msg);
            //////msg.ModificationTime = DateTimeOffset.UtcNow;

            //////context.Update(msg);

            //////await context.SaveChangesAsync();

        }

        /// <summary>
        /// Update a found InMessage (by AS4 Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateInMessageAsync(string messageId, Action<InMessage> updateAction)
        {
            // TODO: this can be optimized

            InMessage inMessage = _dbContext.InMessages
                .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));

            if (inMessage == null)
            {
                LogManager.GetCurrentClassLogger().Error($"No InMessage found for MessageId {messageId}");
                return;
            }

            updateAction(inMessage);
            inMessage.ModificationTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Update a found InException (by AS4 Ref Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateInExceptionAsync(string refToMessageId, Action<InException> updateAction)
        {
            IEnumerable<InException> inExceptions = _dbContext.InExceptions
                .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

            foreach (InException inException in inExceptions)
            {
                updateAction(inException);
                inException.ModificationTime = DateTimeOffset.UtcNow;                
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Update a found OutException (by AS4 Ref Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateOutExceptionAsync(string refToMessageId, Action<OutException> updateAction)
        {

            IEnumerable<OutException> outExceptions = _dbContext.OutExceptions
                .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

            foreach (OutException outException in outExceptions)
            {
                updateAction(outException);
                outException.ModificationTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Update a found <see cref="ReceptionAwareness"/>
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateReceptionAwarenessAsync(string refToMessageId, Action<ReceptionAwareness> updateAction)
        {
            ReceptionAwareness receptionAwareness = _dbContext.ReceptionAwareness
                .FirstOrDefault(a => a.InternalMessageId.Equals(refToMessageId));

            if (receptionAwareness == null)
            {
                LogManager.GetCurrentClassLogger().Error($"Unable to update ReceptionAwareness entity. No record exists for MessageId {refToMessageId}");
                return;
            }
            updateAction(receptionAwareness);

            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IDatastoreRepository
    {
        OutMessage GetOutMessageById(string messageId);
        IEnumerable<OutMessage> GetOutMessagesById(IEnumerable<string> messageIds);
        InMessage GetInMessageById(string messageId);
        ReceptionAwareness GetReceptionAwareness(string messageId);
        IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds);
        Task InsertInExceptionAsync(InException inException);
        Task InsertInMessageAsync(InMessage inMessage);
        Task InsertOutExceptionAsync(OutException outException);
        Task InsertOutMessageAsync(OutMessage outMessage);
        Task InsertReceptionAwarenessAsync(ReceptionAwareness receptionAwareness);

        Task UpdateInMessageAsync(string messageId, Action<InMessage> updateAction);
        Task UpdateOutMessageAsync(string messageId, Action<OutMessage> updateAction);
        Task UpdateInExceptionAsync(string refToMessageId, Action<InException> updateAction);
        Task UpdateOutExceptionAsync(string refToMessageId, Action<OutException> updateAction);
        Task UpdateReceptionAwarenessAsync(string messageId, Action<ReceptionAwareness> updateAction);

        InMessage GetInMessage(Func<InMessage, bool> predicate);

    }
}