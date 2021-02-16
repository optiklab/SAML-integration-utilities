using System.Collections.Generic;

namespace SamlIntegration.Utilities
{
    public class SamlIntegrationSettings
    {
        /// <param name="recipient">Recipient</param>
        /// <param name="issuer">Issuer</param>
        /// <param name="audience">Audience</param>
        /// <param name="certificateThumbprint">Certificate thumbprint</param>
        /// <param name="signAssertion">Whether SAML assertion should be signed or not.</param>
        /// <param name="prependToId">Defines prefix to be added in response id (leave empty or avoid parameter if prefix is not necessary).</param>
        /// <returns>A base64Encoded string with a SAML response.</returns>
        /// <param name="assertionEncryptionCertificateThumbprint">Certificate to encrypt the assertion inside of SAML response.</param>
        public SamlIntegrationSettings(string recipient, string issuer, string audience,
            string certificateThumbprint, bool signAssertion = false, string prependToId = "",
            string assertionEncryptionCertificateThumbprint = "")
        {
            Issuer = issuer.Trim();
            Audience = audience.Trim();
            Recipient = recipient;
            CertificateThumbprint = certificateThumbprint;
            PrependToId = prependToId;
            AssertionEncryptionCertificateThumbprint = assertionEncryptionCertificateThumbprint;
            SignAssertion = signAssertion;

            Attributes = new Dictionary<string, string>();
        }

        public bool SignAssertion { get; private set; }

        public string Recipient { get; private set; }
        public string Issuer { get; private set; }
        public string Audience { get; private set; }
        public string CertificateThumbprint { get; private set; }
        public string PrependToId { get; private set; }
        public string AssertionEncryptionCertificateThumbprint { get; private set; }

        public Dictionary<string, string> Attributes { get; set; }
    }
}
