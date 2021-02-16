using Microsoft.Extensions.Logging;
using SamlIntegration.Utilities.Helpers;
using SamlIntegration.Utilities.Schemas;
using SamlIntegration.Utilities.UserData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SamlIntegration.Utilities
{
    public abstract class SamlIntegrationSteps
    {
        private const string UriFormat = "{0}://{1}";
        private const string IdPrefix = "_";
        private const string XsiSchema = @"http://www.w3.org/2001/XMLSchema-instance";
        private const string XsdSchema = @"http://www.w3.org/2001/XMLSchema";
        private const string LogoutUriParameter = "LogoutUri";
        private const string ReturnUriParameter = "ReturnUri";
        private const string StartUriParameter = "StartUri";

        private const string UserIdParameter = "UserId"; // Custom parameters depend on service providers.

        private readonly ILogger<SamlAssertionAlgorithms> _logger;
        private readonly SamlAssertionAlgorithms _assertionAlgs;
        private readonly SamlResponseAlgorithms _responseAlgs;
        private readonly IUserDataRepository _userDataRepository;

        public SamlIntegrationSteps(ILogger<SamlAssertionAlgorithms> logger,
            SamlAssertionAlgorithms assertionAlgs,
            SamlResponseAlgorithms responseAlgs,
            IUserDataRepository userDataRepository)
        {
            _logger = logger;
            _assertionAlgs = assertionAlgs;
            _responseAlgs = responseAlgs;
            _userDataRepository = userDataRepository;
        }

        public string BuildEncodedSamlResponse(string serviceProviderUri,
            string logoutUri,
            string returnUri,
            string startUri,
            string issuerUri,
            string userId,
            string signingCertificateThumbprint,
            string assertionEncryptionCertificateThumbprint)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { UserIdParameter, userId },
                { LogoutUriParameter, logoutUri },
                { ReturnUriParameter, returnUri },
                { StartUriParameter, startUri }
            };

            Uri audienceUri = new Uri(serviceProviderUri);

            var settings = new SamlIntegrationSettings(
                serviceProviderUri,
                issuerUri,
                string.Format(UriFormat, audienceUri.Scheme, audienceUri.Host),
                signingCertificateThumbprint,
                prependToId: IdPrefix,
                assertionEncryptionCertificateThumbprint: assertionEncryptionCertificateThumbprint);

            settings.Attributes = attributes;

            return BuildAndSignSamlResponse(settings);
        }

        private string BuildAndSignSamlResponse(SamlIntegrationSettings settings)
        {
            AssertionType assertion = SamlAssertionAlgorithms.Create(settings, _userDataRepository);

            var samlResponse = _responseAlgs.Create(settings, assertion);

            var xmlSamlResponse = _responseAlgs.SerializeToXml(samlResponse);

            // Serialize assertion to XML
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlSamlResponse.NameTable);
            namespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            namespaceManager.AddNamespace("xsi", XsiSchema);
            namespaceManager.AddNamespace("xsd", XsdSchema);

            XmlElement xmlAssertion = (XmlElement)xmlSamlResponse.SelectSingleNode("/samlp:Response/saml:Assertion", namespaceManager);

            // Sign assertion
            if (!_assertionAlgs.Sign(settings.CertificateThumbprint, assertion.ID, ref xmlAssertion))
            {
                _logger.LogDebug("Unable to sign SAML assertion!");

                return null;
            }

            // Encrypt assertion
            if (!string.IsNullOrWhiteSpace(settings.AssertionEncryptionCertificateThumbprint) &&
                !_assertionAlgs.Encrypt(settings.AssertionEncryptionCertificateThumbprint, ref xmlSamlResponse))
            {
                _logger.LogDebug("Unable to encrypt SAML assertion!");

                return null;
            }

            // Sign Response
            if (!_responseAlgs.Sign(settings.CertificateThumbprint, samlResponse.ID, ref xmlSamlResponse))
            {
                _logger.LogDebug("Unable to sign SAML response!");

                return null;
            }

            string result = xmlSamlResponse.OuterXml;

            _logger.LogDebug("Result SAML: " + result);

            return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(result));
        }
    }
}
