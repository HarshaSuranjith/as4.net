﻿using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// AS4 Error Codes
    /// </summary>
    public enum ErrorCode
    {
        NotApplicable = 0,

        // ebMS Processing Errors
        Ebms0001 = 1,
        Ebms0002 = 2,
        Ebms0003 = 3,
        Ebms0004 = 4,
        Ebms0005 = 5,
        Ebms0006 = 6,
        Ebms0007 = 7,
        Ebms0008 = 8,
        Ebms0009 = 9,
        Ebms0010 = 10,
        Ebms0011 = 11,

        // Security Processing Errors
        Ebms0101 = 101,
        /// <summary>
        /// Decryption Failed.
        /// </summary>
        Ebms0102 = 102,
        Ebms0103 = 103,

        // Reliable Messaging Errors
        Ebms0201 = 201,
        Ebms0202 = 202,

        // Additional Features Errors
        Ebms0301 = 301,
        Ebms0302 = 302,
        Ebms0303 = 303,
    }

    internal static class ErrorCodeUtils
    {
        public static string GetCategory(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.Ebms0001:
                case ErrorCode.Ebms0002:
                case ErrorCode.Ebms0003:
                case ErrorCode.Ebms0004:
                case ErrorCode.Ebms0011:
                    return "Content";

                case ErrorCode.Ebms0007:
                case ErrorCode.Ebms0008:
                case ErrorCode.Ebms0009:
                    return "Unpackaging";

                case ErrorCode.Ebms0005:
                case ErrorCode.Ebms0006:
                case ErrorCode.Ebms0301:
                case ErrorCode.Ebms0302:
                case ErrorCode.Ebms0303:
                    return "Communication";

                case ErrorCode.Ebms0010:
                    return "Processing";

                default:
                    return null;
            }
        }
      
        private static readonly IDictionary<ErrorAlias, ErrorCode> ErrorCodes = new Dictionary<ErrorAlias, ErrorCode>
        {
            [ErrorAlias.ValueNotRecognized] = ErrorCode.Ebms0001,
            [ErrorAlias.FeatureNotSupported] = ErrorCode.Ebms0002,
            [ErrorAlias.ValueInconsistent] = ErrorCode.Ebms0003,
            [ErrorAlias.Other] = ErrorCode.Ebms0004,
            [ErrorAlias.ConnectionFailure] = ErrorCode.Ebms0005,
            [ErrorAlias.EmptyMessagePartitionChannel] = ErrorCode.Ebms0006,
            [ErrorAlias.MimeInconsistency] = ErrorCode.Ebms0007,
            [ErrorAlias.FeatureNotSupported] = ErrorCode.Ebms0008,
            [ErrorAlias.InvalidHeader] = ErrorCode.Ebms0009,
            [ErrorAlias.ProcessingModeMismatch] = ErrorCode.Ebms0010,
            [ErrorAlias.ExternalPayloadError] = ErrorCode.Ebms0011,
            [ErrorAlias.FailedAuthentication] = ErrorCode.Ebms0101,
            [ErrorAlias.FailedDecryption] = ErrorCode.Ebms0102,
            [ErrorAlias.PolicyNonCompliance] = ErrorCode.Ebms0103,
            [ErrorAlias.MissingReceipt] = ErrorCode.Ebms0301,
            [ErrorAlias.InvalidReceipt] = ErrorCode.Ebms0302,
            [ErrorAlias.DecompressionFailure] = ErrorCode.Ebms0303,
        };

        public static ErrorCode GetErrorCode(ErrorAlias alias)
        {
            if (ErrorCodes.TryGetValue(alias, out var code))
            {
                return code;
            }

            return ErrorCode.Ebms0004;
        }
    }
}
