﻿using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using NLog;
using Error = Eu.EDelivery.AS4.Model.Core.Error;

namespace Eu.EDelivery.AS4.Builders.Core
{
    /// <summary>
    /// Builder to create <see cref="Model.Core.Error"/> Models
    /// </summary>
    public class ErrorBuilder
    {
        private readonly Error _errorMessage;
        private ErrorResult _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBuilder"/> class. 
        /// Start Builder with default settings
        /// </summary>
        public ErrorBuilder()
        {
            _errorMessage = new Error {Errors = new List<ErrorDetail>()};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBuilder"/> class. 
        /// Start Builder with a given AS4 Message Id
        /// </summary>
        /// <param name="messageId">
        /// </param>
        public ErrorBuilder(string messageId)
        {
            _errorMessage = new Error(messageId) {Errors = new List<ErrorDetail>()};
        }

        /// <summary>
        /// Add a AS4 Message Id to the <see cref="Error"/> Model
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public ErrorBuilder WithRefToEbmsMessageId(string messageId)
        {
            _errorMessage.RefToMessageId = messageId;

            return this;
        }

        /// <summary>
        /// Add an error result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public ErrorBuilder WithErrorResult(ErrorResult result)
        {
            _result = result;
            return this;
        }

        /// <summary>
        /// Build the <see cref="Error"/> Model
        /// </summary>
        /// <returns></returns>
        public Error Build()
        {
            if (_result != null)
            {
                _errorMessage.Errors.Add(CreateErrorDetail(_result));
            }

            return _errorMessage;
        }

        private static ErrorDetail CreateErrorDetail(ErrorResult error)
        {
            return new ErrorDetail
            {
                Detail = error.Description,
                Severity = Severity.FAILURE,
                ErrorCode = $"EBMS:{(int)error.Code:0000}",
                Category = ErrorCodeUtils.GetCategory(error.Code),
                ShortDescription = error.GetAliasDescription()
            };
        }

    }
}