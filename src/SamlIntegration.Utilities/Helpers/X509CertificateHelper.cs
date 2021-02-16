using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SamlIntegration.Utilities
{
    public static class X509CertificateHelper
    {
        /// <summary>
        /// Returns first certificate in specified store found by thumbprint.
        /// </summary>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static X509Certificate2 GetCertificateByThumbprint(string thumbprint, StoreName storeName, StoreLocation storeLocation)
        {
            X509Certificate2 certificate = null;
            X509Store store = null;

            try
            {
                store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection storeCollection = store.Certificates;
                X509Certificate2Collection certificates = storeCollection.Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (certificates.Count > 0)
                    certificate = certificates[0];  // Take first and work done.
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }

            return certificate;
        }
    }
}
