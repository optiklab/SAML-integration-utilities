using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SamlIntegration.Utilities.Helpers
{
    public class SigningHelper
    {
        /// <summary>
        /// Signs an XML Document for a Saml Response.
        /// </summary>
        /// <exception cref="CryptographicException"></exception>
        /// <exception cref="CryptographicUnexpectedOperationException"></exception>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="System.Xml.XmlException"></exception>
        /// <exception cref="System.Xml.XPath.XPathException"></exception>
        public static SamlSignedXml SignXml(XmlDocument doc, X509Certificate2 certificate, string referenceId, string referenceValue)
        {
            var samlSignedXml = new SamlSignedXml(doc, referenceId);

            return SignXml(samlSignedXml, certificate, referenceValue);
        }

        /// <summary>
        /// Signs an XML Element of a Saml Response.
        /// </summary>
        /// <exception cref="CryptographicException"></exception>
        /// <exception cref="CryptographicUnexpectedOperationException"></exception>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="System.Xml.XmlException"></exception>
        /// <exception cref="System.Xml.XPath.XPathException"></exception>
        public static SamlSignedXml SignXml(XmlElement element, X509Certificate2 certificate, string referenceId, string referenceValue)
        {
            var samlSignedXml = new SamlSignedXml(element, referenceId);

            return SignXml(samlSignedXml, certificate, referenceValue);
        }

        private static SamlSignedXml SignXml(SamlSignedXml samlSignedXml, X509Certificate2 certificate, string referenceValue)
        {
            samlSignedXml.SigningKey = certificate.PrivateKey;
            samlSignedXml.SignedInfo.CanonicalizationMethod = SamlSignedXml.XmlDsigExcC14NTransformUrl;

            // Create a reference to be signed. 
            Reference reference = new Reference
            {
                Uri = "#" + referenceValue
            };

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());

            // Add the reference to the SignedXml object. 
            samlSignedXml.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate). 
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));

            samlSignedXml.KeyInfo = keyInfo;

            // Compute the signature. 
            samlSignedXml.ComputeSignature();

            return samlSignedXml;
        }
    }
}
