﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;
using MimeKit.IO;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Attachment
    {
        public string Id { get; set; }
        public string ContentType { get; set; }

        private Stream _content;

        [XmlIgnore]
        public Stream Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (ReferenceEquals(_content, value) == false)
                {
                    _content?.Dispose();
                }
                _content = value;

                if (_content != null)
                {
                    EstimatedContentSize = StreamUtilities.GetStreamSize(_content);
                }
                else
                {
                    EstimatedContentSize = -1;
                }
            }
        }

        [XmlIgnore]
        public long EstimatedContentSize
        {
            get;
            private set;
        }

        public string Location { get; set; }
        public List<Schema> Schemas { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public Attachment() : this(IdentifierFactory.Instance.Create())
        {
        }

        public Attachment(string id)
        {
            Id = id;
            InitializeDefaults();
        }

        /// <summary>
        /// Verifies if this is the Attachment that is referenced by the given <paramref name="partInfo"/>
        /// </summary>
        /// <param name="partInfo"></param>
        /// <returns></returns>
        public bool Matches(PartInfo partInfo)
        {
            return partInfo.Href != null && partInfo.Href.Equals($"cid:{Id}");
        }

        /// <summary>
        /// Verifies if this is the Attachment that is referenced by the given cryptography <paramref name="reference"/>
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public bool Matches(System.Security.Cryptography.Xml.Reference reference)
        {
            return reference.Uri.Equals($"cid:{Id}");
        }

        /// <summary>
        /// Makes sure that the Attachment Content is positioned at the start of the content.
        /// </summary>
        public void ResetContentPosition()
        {
            if (Content != null)
            {
                StreamUtilities.MovePositionToStreamStart(Content);
            }
        }

        private void InitializeDefaults()
        {
            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Schemas = new List<Schema>();
            ContentType = "application/octet-stream";
            EstimatedContentSize = -1;
        }
    }

    public class Schema
    {
        public string Location { get; set; }
        public string Version { get; set; }
        public string Namespace { get; set; }
    }
}