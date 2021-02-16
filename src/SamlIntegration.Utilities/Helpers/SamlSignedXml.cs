using System.Security.Cryptography.Xml;
using System.Xml;

namespace SamlIntegration.Utilities
{
    /// <summary>
    /// SamlSignedXml - Class is used to sign xml, basically the when the ID is retreived the correct ID is used.  
    /// without this, the id reference would not be valid.
    /// </summary>
    public class SamlSignedXml : SignedXml
    {
        private string _referenceAttributeId = string.Empty;

        public SamlSignedXml(XmlDocument document, string referenceAttributeId) : base(document)
        {
            _referenceAttributeId = referenceAttributeId;
        }

        public SamlSignedXml(XmlElement element, string referenceAttributeId) : base(element)
        {
            _referenceAttributeId = referenceAttributeId;
        }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            return (XmlElement)document.SelectSingleNode(string.Format("//*[@{0}='{1}']", _referenceAttributeId, idValue));
        }
    }
}
