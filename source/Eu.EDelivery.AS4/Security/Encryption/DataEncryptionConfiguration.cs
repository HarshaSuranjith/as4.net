﻿using Eu.EDelivery.AS4.Security.Transforms;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    public class DataEncryptionConfiguration
    {

        public static readonly DataEncryptionConfiguration Default =
            new DataEncryptionConfiguration(Model.PMode.Encryption.Default.Algorithm,
                                            Model.PMode.Encryption.Default.AlgorithmKeySize);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEncryptionConfiguration"/> class.
        /// </summary>
        /// <param name="encryptionMethod">The encryption method.</param>
        /// <param name="algorithmKeySize">Size of the algorithm key.</param>
        /// <param name="encryptionType">Type of the encryption.</param>
        /// <param name="transformAlgorithm">The transform algorithm.</param>
        public DataEncryptionConfiguration(
            string encryptionMethod,
            int algorithmKeySize,
            string encryptionType = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Only",
            string transformAlgorithm = AttachmentCiphertextTransform.Url)
        {
            EncryptionMethod = encryptionMethod;
            EncryptionType = encryptionType;
            TransformAlgorithm = transformAlgorithm;
            AlgorithmKeySize = algorithmKeySize;
        }

        /// <summary>
        /// Gets the encryption method.
        /// </summary>
        /// <value>The encryption method.</value>
        public string EncryptionMethod { get; }

        /// <summary>
        /// Gets the type of the encryption.
        /// </summary>
        /// <value>The type of the encryption.</value>
        public string EncryptionType { get; }

        /// <summary>
        /// Gets the transform algorithm.
        /// </summary>
        /// <value>The transform algorithm.</value>
        public string TransformAlgorithm { get; }

        /// <summary>
        /// Gets or sets the size of the algorithm key.
        /// </summary>
        /// <value>The size of the algorithm key.</value>
        public int AlgorithmKeySize { get; }
    }
}