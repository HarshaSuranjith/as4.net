﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// <see cref="AS4Message" /> Serializer to Xml
    /// </summary>
    public static class AS4XmlSerializer
    {
        private static readonly IDictionary<Type, XmlSerializer> Serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        /// Serialize Model into Xml String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize<T>(T data)
        {
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, DefaultXmlWriterSettings))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(xmlWriter, data);
                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Serialize a <see cref="AS4Message"/> to a <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="as4Message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static XmlDocument Serialize(AS4Message as4Message, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                var provider = new SerializerProvider();

                ISerializer serializer = provider.Get(Constants.ContentTypes.Soap);
                serializer.Serialize(as4Message, memoryStream, cancellationToken);

                return LoadEnvelopeToDocument(memoryStream);
            }
        }

        private static readonly XmlWriterSettings DefaultXmlWriterSettings =
            new XmlWriterSettings
            {
                CloseOutput = false,
                Encoding = new UTF8Encoding(false)
            };

        private static XmlDocument LoadEnvelopeToDocument(Stream envelopeStream)
        {
            envelopeStream.Position = 0;
            var envelopeXmlDocument = new XmlDocument() {PreserveWhitespace = true};

            envelopeXmlDocument.Load(envelopeStream);
            

            return envelopeXmlDocument;
        }

        private static readonly XmlReaderSettings DefaultXmlReaderSettings =
            new XmlReaderSettings
            {
                Async = true,
                CloseInput = false,
                IgnoreComments = true,            
            };

        /// <summary>
        /// Deserialize Xml String to Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml) where T : class
        {
            if (xml == null)
            {
                return null;
            }
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                var serializer = new XmlSerializer(typeof(T));
                if (serializer.CanDeserialize(reader))
                {
                    return serializer.Deserialize(reader) as T;
                }

                return null;
            }
        }

        /// <summary>
        /// Deserialize to Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Deserialize<T>(XmlReader reader) where T : class
        {
            XmlSerializer serializer = GetSerializerForType(typeof(T));
            return serializer.Deserialize(reader) as T;
        }

        private static XmlSerializer GetSerializerForType(Type type)
        {
            if (!Serializers.ContainsKey(type))
                Serializers[type] = new XmlSerializer(type);
            return Serializers[type];
        }
    }
}