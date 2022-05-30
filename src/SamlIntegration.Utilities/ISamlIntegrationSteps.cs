namespace SamlIntegration.Utilities
{
    public interface ISamlIntegrationSteps
    {
        /// <summary>
        /// Builds authentication request data that is singed, encrypted and ready for use
        /// by rules of the SAML 2.0 integraton protocol.
        /// </summary>
        /// <returns>String of data ready to be written into POST request.</returns>
        string BuildEncodedSamlResponse();
    }
}
