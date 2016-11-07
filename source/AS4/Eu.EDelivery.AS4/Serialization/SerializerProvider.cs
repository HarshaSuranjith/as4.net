﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Interface to provide <see cref="ISerializer"/> implementations
    /// </summary>
    public interface ISerializerProvider
    {
        ISerializer Get(string contentType);
    }

    /// <summary>
    /// Class to provide <see cref="ISerializer"/> implementations
    /// </summary>
    public class SerializerProvider : ISerializerProvider
    {
        private readonly IDictionary<string, ISerializer> _serializers;

        internal SerializerProvider()
        {
            this._serializers = new Dictionary<string, ISerializer>();

            var soapSerializer = new SoapEnvelopeSerializer();
            this._serializers.Add(Constants.ContentTypes.Soap, soapSerializer);
            this._serializers.Add(Constants.ContentTypes.Mime, new MimeMessageSerializer(soapSerializer));
        }

        /// <summary>
        /// Get the <see cref="ISerializer"/> implementation
        /// based on a given Content Type
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ISerializer Get(string contentType)
        {
            foreach (string key in this._serializers.Keys)
                if (KeyMatchesContentType(contentType, key))
                    return this._serializers[key];

            throw new AS4Exception($"No given Serializer found for a given Content Type: {contentType}");
        }

        private bool KeyMatchesContentType(string contentType, string key)
        {
            return key.Equals(contentType) || Regex.IsMatch(contentType, key, RegexOptions.IgnoreCase);
        }
    }
}
