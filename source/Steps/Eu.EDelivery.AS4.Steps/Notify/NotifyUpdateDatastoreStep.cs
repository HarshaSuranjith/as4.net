using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    [Obsolete("The update functionality is now moved to the " + nameof(SendNotifyMessageStep))]
    [Info("Update datastore after notification")]
    [Description("This step makes sure that the status of the message is set to �Notified� after notification")]
    public class NotifyUpdateDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateDatastoreStep" /> class.
        /// </summary>
        public NotifyUpdateDatastoreStep() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateDatastoreStep" /> class.
        /// </summary>
        public NotifyUpdateDatastoreStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            var notifyMessage = messagingContext.NotifyMessage;
            Logger.Info($"{messagingContext.LogTag} Mark the stored notify message as Notified");

            await UpdateDatastoreAsync(notifyMessage, messagingContext).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task UpdateDatastoreAsync(NotifyMessageEnvelope notifyMessage, MessagingContext messagingContext)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                if (notifyMessage.EntityType == typeof(InMessage))
                {
                    Logger.Debug(messagingContext.LogTag + "Update InMessage with Status and Operation set to Notified");
                    repository.UpdateInMessage(notifyMessage.MessageInfo.MessageId, m =>
                    {
                        m.SetStatus(InStatus.Notified);
                        m.Operation = Operation.Notified;
                    });
                }
                else if (notifyMessage.EntityType == typeof(OutMessage) && messagingContext.MessageEntityId != null)
                {
                    Logger.Debug(messagingContext.LogTag + "Update OutMessage with Status and Operation set to Notified");
                    repository.UpdateOutMessage(messagingContext.MessageEntityId.Value, m =>
                    {
                        m.SetStatus(OutStatus.Notified);
                        m.Operation = Operation.Notified;
                    });
                }
                else if (notifyMessage.EntityType == typeof(InException))
                {
                    Logger.Debug(messagingContext.LogTag + "Update InException with Status and Operation set to Notified");
                    repository.UpdateInException(notifyMessage.MessageInfo.RefToMessageId, ex => ex.Operation = Operation.Notified);
                }
                else if (notifyMessage.EntityType == typeof(OutException))
                {
                    Logger.Debug(messagingContext.LogTag + "Update OutException with Status and Operation set to Notified");
                    repository.UpdateOutException(notifyMessage.MessageInfo.RefToMessageId, ex => ex.Operation = Operation.Notified);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unable to update notified entities of type {notifyMessage.EntityType.FullName}." +
                        "Please provide one of the following types in the notify message: " +
                        "InMessage, OutMessage, InException, and OutException are supported.");
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}