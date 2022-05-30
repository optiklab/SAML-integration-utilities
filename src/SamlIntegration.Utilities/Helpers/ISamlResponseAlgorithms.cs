using SamlIntegration.Utilities.Schemas;
using System.Xml;

namespace SamlIntegration.Utilities.Helpers
{
    public interface ISamlResponseAlgorithms
    {
        ResponseType Create(SamlIntegrationSettings samlResponseSpecification, AssertionType assertion);
        bool Sign(string thumbprint, string responseId, ref XmlDocument xmlSamlResponse);
        XmlDocument SerializeToXml(ResponseType samlResponse);
    }
}
