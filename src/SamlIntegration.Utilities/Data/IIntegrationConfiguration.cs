namespace SamlIntegration.Utilities.Data
{
    public interface IIntegrationConfiguration
    {
        string ServiceProviderUri { get; }
        string LogoutUri { get; }
        string ReturnUri { get; }
        string StartUri { get; }
        string IssuerUri { get; }
        string SigningCertificateThumbprint { get; }
        string AssertionEncryptionCertificateThumbprint { get; }
    }
}
