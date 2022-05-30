using SamlIntegration.Utilities.Schemas;
using SamlIntegration.Utilities.Data;
using System.Xml;

namespace SamlIntegration.Utilities.Helpers
{
    public interface ISamlAssertionAlgorithms
    {
        AssertionType Create(SamlIntegrationSettings settings, IUserDataRepository userData);
        bool Sign(string thumbprint, string responseId, ref XmlElement xmlAssertion);
        bool Encrypt(string thumbprint, ref XmlDocument xmlDocument);
    }
}
