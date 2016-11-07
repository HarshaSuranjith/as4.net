﻿using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Builders.Core
{
    /// <summary>
    /// Builder to create <see cref="Error"/> Models
    /// </summary>
    public class ErrorBuilder
    {
        private readonly Error _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBuilder"/> class. 
        /// Start Builder with default settings
        /// </summary>
        public ErrorBuilder()
        {
            this._errorMessage = new Error();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBuilder"/> class. 
        /// Start Builder with a given AS4 Message Id
        /// </summary>
        /// <param name="messageId">
        /// </param>
        public ErrorBuilder(string messageId)
        {
            this._errorMessage = new Error(messageId);
        }

        /// <summary>
        /// Add a AS4 Message Id to the <see cref="Error"/> Model
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public ErrorBuilder WithRefToEbmsMessageId(string messageId)
        {
            this._errorMessage.RefToMessageId = messageId;

            return this;
        }

        /// <summary>
        /// Add a <see cref="AS4Exception"/> details
        /// to the <see cref="Error"/> Message
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public ErrorBuilder WithAS4Exception(AS4Exception exception)
        {
            this._errorMessage.Exception = exception;
            this._errorMessage.Errors = CreateErrorDetails(exception);

            return this;
        }

        private IList<ErrorDetail> CreateErrorDetails(AS4Exception exception)
        {
            var errorDetails = new List<ErrorDetail>();
            foreach (string messageId in exception.MessageIds)
            {
                ErrorDetail detail = CreateErrorDetail(exception);
                detail.RefToMessageInError = messageId;
                errorDetails.Add(detail);
            }

            return errorDetails;
        }

        private ErrorDetail CreateErrorDetail(AS4Exception exception)
        {
            return new ErrorDetail
            {
                Detail = exception.Message,
                Severity = Severity.FAILURE,
                ErrorCode = $"EBMS:{(int) exception.ErrorCode:0000}",
            };
        }

        /// <summary>
        /// Build the <see cref="Error"/> Model
        /// </summary>
        /// <returns></returns>
        public Error Build()
        {
            if (!string.IsNullOrEmpty(this._errorMessage.MessageId))
                this._errorMessage.Exception?.AddMessageId(this._errorMessage.MessageId);

            this._errorMessage.Timestamp = DateTimeOffset.UtcNow;
            return this._errorMessage;
        }

        public Error BuildWithOriginalAS4Exception()
        {
            this._errorMessage.Timestamp = DateTimeOffset.UtcNow;
            return this._errorMessage;
        }
    }
}