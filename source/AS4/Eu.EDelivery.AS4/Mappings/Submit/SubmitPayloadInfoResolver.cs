﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// <see cref="SubmitMessage"/> Resolver to get the <see cref="PartInfo"/> Models
    /// </summary>
    public class SubmitPayloadInfoResolver : ISubmitResolver<List<PartInfo>>
    {
        public static readonly SubmitPayloadInfoResolver Default = new SubmitPayloadInfoResolver();

        /// <summary>
        /// Resolve the <see cref="PartyInfo"/>
        /// 1. SubmitMessage / Payloads / Payload[n] / Id
        /// 2. Generated according to Settings / GuidFormat
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public List<PartInfo> Resolve(SubmitMessage submitMessage)
        {
            if (submitMessage.Payloads == null)
            {
                return new List<PartInfo>();
            }

            return ResolvePartInfosFromSubmitMessage(submitMessage).ToList();
        }

        private static IEnumerable<PartInfo> ResolvePartInfosFromSubmitMessage(SubmitMessage submitMessage)
        {
            bool submitContainsDuplicatePayloadIds = 
                submitMessage.Payloads.GroupBy(p => p.Id).All(g => g.Count() == 1) == false;

            if (submitContainsDuplicatePayloadIds)
            {
                throw new InvalidDataException("Invalid Payloads: duplicate Payload Ids");
            }

            return submitMessage.Payloads.Select(p => CreatePartInfo(p, submitMessage));
        }

        private static PartInfo CreatePartInfo(Payload submitPayload, SubmitMessage submit)
        {
            string href = submitPayload.Id ?? IdentifierFactory.Instance.Create();

            var returnPayload = new PartInfo(href.StartsWith("cid:") ? href : $"cid:{href}");

            if (submitPayload.Schemas != null)
            {
                returnPayload.Schemas = submitPayload.Schemas
                    .Select(SubmitToAS4Schema)
                    .ToList();
            }

            if (submitPayload.PayloadProperties != null)
            {
                returnPayload.Properties = submitPayload.PayloadProperties
                    .Select(SubmitToProperty)
                    .Concat(CompressionProperties(submitPayload, submit))
                    .ToDictionary(t => t.propName, t => t.propValue);
            }

            return returnPayload;
        }

        private static Model.Core.Schema SubmitToAS4Schema(Model.Common.Schema sch)
        {
            var schema = AS4Mapper.Map<Model.Core.Schema>(sch);
            if (string.IsNullOrEmpty(schema.Location))
            {
                throw new InvalidDataException("Invalid Schema: Schema needs a location");
            }

            return schema;
        }

        private static (string propName, string propValue) SubmitToProperty(PayloadProperty prop)
        {
            if (string.IsNullOrEmpty(prop.Name))
            {
                throw new InvalidDataException("Invalid Payload Property: Property requires name");
            }

            return (prop.Name, prop.Value);
        }

        private static IEnumerable<(string propName, string propValue)> CompressionProperties(
            Payload payload, 
            SubmitMessage submitMessage)
        {
            if (submitMessage.PMode.MessagePackaging.UseAS4Compression)
            {
                return new[]
                {
                    ("CompressionType", "application/gzip"),
                    ("MimeType", !string.IsNullOrEmpty(payload.MimeType)
                        ? payload.MimeType
                        : "application/octet-stream")
                };
            }

            return Enumerable.Empty<(string, string)>();
        }
    }
}
