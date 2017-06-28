﻿using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class InboundExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHandler"/> class.
        /// </summary>
        public InboundExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public InboundExceptionHandler(Func<DatastoreContext> createContext)
        {
            _createContext = createContext;
        }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, Stream contents)
        {
            Logger.Error(exception.Message);

            await InsertInException(exception, inException => inException.MessageBody = contents.ToBytes());
            return new MessagingContext(exception);
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            Logger.Error(exception.Message);

            return await HandleExecutionException(exception, context);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            Logger.Error(exception.Message);
            bool isSubmitMessage = context.SubmitMessage != null;

            Action<InException> updateException = ex => { };

            if (isSubmitMessage)
            {
                await InsertInException(exception, context, updateException);

                updateException = ex => ex.MessageBody = AS4XmlSerializer.TryToXmlBytesAsync(context.SubmitMessage).Result;
            }
            else
            {
                string messageId = context.EbmsMessageId;
                await SideEffectRepositoryUsage(
                   repository => repository.UpdateInMessage(messageId, m => m.Status = InStatus.Exception));
                updateException = ex => ex.EbmsRefToMessageId = messageId;
            }

            await InsertInException(exception, context, updateException);
            return new MessagingContext(exception);
        }

        private async Task InsertInException(Exception exception, MessagingContext context, Action<InException> updateException)
        {
            await InsertInException(
                exception,
                inException =>
                {
                    updateException(inException);

                    inException.PMode = AS4XmlSerializer.ToString(context.ReceivingPMode);
                    inException.Operation =
                        context.ReceivingPMode?.ExceptionHandling?.NotifyMessageConsumer == true
                            ? Operation.ToBeNotified
                            : default(Operation);
                });
        }

        private async Task InsertInException(Exception exception, Action<InException> alterException)
        {
            await SideEffectRepositoryUsage(
                repository =>
                {
                    var inException = new InException
                    {
                        Exception = exception.Message,
                        InsertionTime = DateTimeOffset.Now,
                        ModificationTime = DateTimeOffset.Now
                    };
                    alterException(inException);

                    repository.InsertInException(inException);
                });
        }

        private async Task SideEffectRepositoryUsage(Action<DatastoreRepository> usage)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                usage(repository);
                await context.SaveChangesAsync();
            }
        }
    }
}

