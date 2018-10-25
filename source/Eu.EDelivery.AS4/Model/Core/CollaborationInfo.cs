﻿using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class CollaborationInfo
    {
        public Maybe<AgreementReference> AgreementReference { get; }

        public Service Service { get; }

        public string Action { get; }

        public string ConversationId { get; }

        public static readonly string DefaultConversationId = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="agreement"></param>
        public CollaborationInfo(AgreementReference agreement)
            : this(
                agreement: agreement,
                service: Service.TestService,
                action: Constants.Namespaces.TestAction,
                conversationId: DefaultConversationId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="service"></param>
        internal CollaborationInfo(Service service) 
            : this(
                Maybe<AgreementReference>.Nothing, 
                service, 
                Constants.Namespaces.TestAction, 
                DefaultConversationId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="action"></param>
        internal CollaborationInfo(Service service, string action)
            : this(
                Maybe<AgreementReference>.Nothing,
                service,
                action,
                DefaultConversationId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="service"></param>
        /// <param name="action"></param>
        /// <param name="conversationId"></param>
        public CollaborationInfo(
            AgreementReference agreement,
            Service service,
            string action,
            string conversationId)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException(nameof(agreement));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            AgreementReference = Maybe.Just(agreement);
            Service = service;
            Action = action;
            ConversationId = conversationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="service"></param>
        /// <param name="action"></param>
        /// <param name="conversationId"></param>
        public CollaborationInfo(
            Maybe<AgreementReference> agreement, 
            Service service, 
            string action, 
            string conversationId)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException(nameof(agreement));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            AgreementReference = agreement;
            Service = service;
            Action = action;
            ConversationId = conversationId;
        }
    }
}