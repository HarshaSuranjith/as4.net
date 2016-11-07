﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Builders.Core
{
    /// <summary>
    /// Builder to create <see cref="AS4Exception"/> Models
    /// with multiple configurations
    /// </summary>
    public class AS4ExceptionBuilder
    {
        private readonly IList<string> _messageIds;

        private string _description;
        private Exception _innerException;
        private ErrorCode _errorCode;
        private ExceptionType _exceptionType;
        private string _pmodeString;

        private AS4Exception _as4Exception;

        public AS4ExceptionBuilder()
        {
            this._messageIds = new List<string>();
        }

        /// <summary>
        /// Add a description to the <see cref="AS4Exception"/>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public AS4ExceptionBuilder WithDescription(string description)
        {
            this._description = description;

            return this;
        }

        public AS4ExceptionBuilder WithInnerException(Exception innerException)
        {
            this._innerException = innerException;

            var as4Exception = this._innerException as AS4Exception;
            if (as4Exception == null) return this;
            AssignPublicProperties(as4Exception);

            return this;
        }

        private void AssignPublicProperties(AS4Exception as4Exception)
        {
            as4Exception.MessageIds.ForEach(i =>
            {
                if (!this._messageIds.Contains(i))
                    this._messageIds.Add(i);
            });

            if (as4Exception.ErrorCode != default(ErrorCode)) this._errorCode = as4Exception.ErrorCode;
            if (as4Exception.ExceptionType != default(ExceptionType)) this._exceptionType = as4Exception.ExceptionType;
            if (!string.IsNullOrEmpty(as4Exception.PMode)) this._pmodeString = as4Exception.PMode;
        }

        /// <summary>
        /// Add message Ids to the <see cref="AS4Exception"/>
        /// </summary>
        /// <param name="messageIds"></param>
        /// <returns></returns>
        public AS4ExceptionBuilder WithMessageIds(params string[] messageIds)
        {
            messageIds.ForEach(i => this._messageIds.Add(i));

            return this;
        }

        /// <summary>
        /// Assign an <see cref="ErrorCode"/> to the <see cref="AS4Exception"/>
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public AS4ExceptionBuilder WithErrorCode(ErrorCode errorCode)
        {
            this._errorCode = errorCode;

            return this;
        }

        /// <summary>
        /// Assign a <see cref="ExceptionType"/> to the <see cref="AS4Exception"/>
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public AS4ExceptionBuilder WithExceptionType(ExceptionType exceptionType)
        {
            this._exceptionType = exceptionType;

            return this;
        }

        /// <summary>
        /// Add a serialized PMode to the <see cref="AS4Exception"/>
        /// </summary>
        /// <param name="pmodeString"></param>
        /// <returns></returns>
        public AS4ExceptionBuilder WithPModeString(string pmodeString)
        {
            this._pmodeString = pmodeString;

            return this;
        }

        public AS4ExceptionBuilder WithReceivingPMode(ReceivingProcessingMode pmode)
        {
            this._pmodeString = AS4XmlSerializer.Serialize(pmode);

            return this;
        }

        public AS4ExceptionBuilder WithSendingPMode(SendingProcessingMode pmode)
        {
            this._pmodeString = AS4XmlSerializer.Serialize(pmode);

            return this;
        }

        /// <summary>
        /// Start from an existing <see cref="AS4Exception"/>
        /// and build from there
        /// </summary>
        /// <param name="as4Exception"></param>
        /// <returns></returns>
        public AS4ExceptionBuilder WithExistingAS4Exception(AS4Exception as4Exception)
        {
            if (as4Exception == null)
                throw new ArgumentNullException(nameof(as4Exception));

            this._as4Exception = as4Exception;
            return this;
        }

        /// <summary>
        /// Build the <see cref="AS4Exception"/>
        /// with a configured items
        /// </summary>
        /// <returns></returns>
        public AS4Exception Build()
        {
            if (this._as4Exception == null)
                this._as4Exception = new AS4Exception(this._description, this._innerException);

            AssignPublicProperties();

            return this._as4Exception;
        }

        private void AssignPublicProperties()
        {
            this._as4Exception.ErrorCode = this._errorCode;
            this._as4Exception.MessageIds = this._messageIds.ToArray();
            this._as4Exception.ExceptionType = this._exceptionType;
            this._as4Exception.PMode = this._pmodeString;
        }
    }
}