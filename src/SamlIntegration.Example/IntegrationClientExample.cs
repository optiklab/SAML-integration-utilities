using Microsoft.Extensions.Logging;
using SamlIntegration.Utilities;
using SamlIntegration.Utilities.Data;
using SamlIntegration.Utilities.Helpers;
using System;
using System.Net;
using System.IO;

namespace SamlIntegration.Example
{
    public class IntegrationClientExample : IIntegrationClient
    {
        private readonly ILogger<SamlIntegrationSteps> _logger;
        private readonly SamlAssertionAlgorithms _samlAssertionAlgorithms;
        private readonly SamlResponseAlgorithms _samlResponseAlgorithms;
        private readonly IUserDataRepository _userDataRepository;
        private readonly IIntegrationConfiguration _configuration;

        public IntegrationClientExample(ILogger<SamlIntegrationSteps> logger,
            SamlAssertionAlgorithms samlAssertionAlgorithms,
            SamlResponseAlgorithms samlResponseAlgorithms,
            IUserDataRepository userDataRepository,
            IIntegrationConfiguration configuration)
        {
            _logger = logger;
            _samlAssertionAlgorithms = samlAssertionAlgorithms;
            _samlResponseAlgorithms = samlResponseAlgorithms;
            _userDataRepository = userDataRepository;
            _configuration = configuration;
        }

        public Uri GetRedirectUrl()
        {
            // Prepare authentication request data according to SAML 2.0
            var steps = new SamlIntegrationSteps(
                _logger,
                _samlAssertionAlgorithms,
                _samlResponseAlgorithms,
                _userDataRepository,
                _configuration);

            string samlResponse = steps.BuildEncodedSamlResponse();

            // Form a POST method
            var request = (HttpWebRequest)WebRequest.Create("");
            request.Method = "POST";

            string requestData = string.Format("SAMLResponse={0}", samlResponse);
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(requestData);
                writer.Flush();
            }

            // TODO Fill UserAgent, Cookies, any other HTTP headers.

            HttpWebResponse response = null;
            string body;
            try
            {
                response = (HttpWebResponse)request.GetResponse();

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    body = reader.ReadToEnd();
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            // TODO Read/Parse body to retrieve redirect URL.
            return new Uri(body);
        }
    }
}
