﻿using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Repositories
{
    /// <summary>
    /// Repository to expose the Certificate from the Certificate Store
    /// </summary>
    [Info("Certificate repository")]
    public class CertificateRepository : ICertificateRepository
    {
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateRepository"/> class
        /// with default Configuration
        /// </summary>
        public CertificateRepository() : this(Config.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateRepository"/> class
        /// Create a Certificate Repository with a given Configuration
        /// </summary>
        /// <param name="config">
        /// </param>
        public CertificateRepository(IConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        /// <summary>
        /// Get the <see cref="X509Certificate2"/>
        /// from the Certificate Store
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="privateKeyReference"></param>
        /// <returns></returns>
        public X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference)
        {
            using (X509Store certificateStore = GetCertificateStore())
            {
                certificateStore.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificateCollection =
                    certificateStore.Certificates.Find(findType, privateKeyReference, validOnly: false);

                if (certificateCollection.Count <= 0)
                {
                    throw new CryptographicException(
                          $"Could not find certificate in store: {_config.CertificateStore} where {findType} is {privateKeyReference}");
                }

                return certificateCollection[0];
            }
        }

        private X509Store GetCertificateStore()
        {
            string storeName = _config.CertificateStore;
            return new X509Store(storeName, StoreLocation.LocalMachine);
        }
    }

    public interface ICertificateRepository
    {
        /// <summary>
        /// Find a <see cref="X509Certificate2"/> based on the given <paramref name="privateKeyReference"/> for the <paramref name="findType"/>.
        /// </summary>
        /// <param name="findType">Kind of searching approach.</param>
        /// <param name="privateKeyReference">Value to search in the repository.</param>
        /// <returns></returns>
        X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference);
    }
}