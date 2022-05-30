using Microsoft.Extensions.Logging;
using SamlIntegration.Utilities.Helpers;
using SamlIntegration.Utilities.Schemas;
using SamlIntegration.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SamlIntegration.Utilities
{
    public class SamlIntegrationSteps : ISamlIntegrationSteps
    {
        private const string UriFormat = "{0}://{1}";
        private const string IdPrefix = "_";
        private const string XsiSchema = @"http://www.w3.org/2001/XMLSchema-instance";
        private const string XsdSchema = @"http://www.w3.org/2001/XMLSchema";
        private const string LogoutUriParameter = "LogoutUri";
        private const string ReturnUriParameter = "ReturnUri";
        private const string StartUriParameter = "StartUri";

        // An example of custom parameters used for integration with third-party.
        private const string UserIdParameter = "UserId";

        private readonly ILogger<SamlIntegrationSteps> _logger;
        private readonly SamlAssertionAlgorithms _assertionAlgs;
        private readonly SamlResponseAlgorithms _responseAlgs;
        private readonly IUserDataRepository _userDataRepository;
        private readonly IIntegrationConfiguration _configuration;

        public SamlIntegrationSteps(ILogger<SamlIntegrationSteps> logger,
            SamlAssertionAlgorithms assertionAlgs,
            SamlResponseAlgorithms responseAlgs,
            IUserDataRepository userDataRepository,
            IIntegrationConfiguration configuration)
        {
            _logger = logger;
            _assertionAlgs = assertionAlgs;
            _responseAlgs = responseAlgs;
            _userDataRepository = userDataRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Builds authentication request data that is singed, encrypted and ready for use
        /// by rules of the SAML 2.0 integraton protocol.
        /// </summary>
        /// <returns>String of data ready to be written into POST request.</returns>
        public string BuildEncodedSamlResponse()
        {
            // NOTE! This is just an example.
            // The list of actual required attributes depends on the third-party vendor requirements.
            Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { UserIdParameter, _userDataRepository.GetUserGuid() },
                { LogoutUriParameter, _configuration.LogoutUri },
                { ReturnUriParameter, _configuration.ReturnUri },
                { StartUriParameter, _configuration.StartUri }
            };

            Uri audienceUri = new Uri(_configuration.ServiceProviderUri);

            var settings = new SamlIntegrationSettings(
                _configuration.ServiceProviderUri,
                _configuration.IssuerUri,
                string.Format(UriFormat, audienceUri.Scheme, audienceUri.Host),
                _configuration.SigningCertificateThumbprint,
                prependToId: IdPrefix,
                assertionEncryptionCertificateThumbprint: _configuration.AssertionEncryptionCertificateThumbprint);

            settings.Attributes = attributes;

            return BuildAndSignSamlResponse(settings);
        }

        private string BuildAndSignSamlResponse(SamlIntegrationSettings settings)
        {
            AssertionType assertion = _assertionAlgs.Create(settings, _userDataRepository);

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
