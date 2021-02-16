using SamlIntegration.Utilities;
using SamlIntegration.Utilities.Helpers;
using SamlIntegration.Utilities.Schemas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SamlIntegration.MetadataFileGenerator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Dictionary<string, string> metadataAttributes = new Dictionary<string, string>();

            Console.WriteLine("Please type in parameters...");
            Console.WriteLine("Example:");
            Console.WriteLine("Partner-id: 123");
            Console.WriteLine("User-email: test@example.com");
            Console.WriteLine("Login-id: testuser1");
            Console.WriteLine("Platform: desktop");
            Console.WriteLine("EntityID: https://webstore.com/");
            Console.WriteLine("Thumbprint (optional, leave empty if signing not used): BDEBC9D4C82CE8798EA360FE45E0E6E95DF5F659");
            Console.WriteLine(string.Empty);

            Console.Write("partner-id: ");
            metadataAttributes.Add("partner-id", Console.ReadLine());
            Console.Write("user-email: ");
            metadataAttributes.Add("user-email", Console.ReadLine());
            Console.Write("login-id: ");
            metadataAttributes.Add("login-id", Console.ReadLine());
            Console.Write("platform: ");
            metadataAttributes.Add("platform", Console.ReadLine());
            Console.Write("entityID: ");
            string entityId = Console.ReadLine();
            Console.Write("Certificate Thumbprint (optional): ");
            string thumbprint = Console.ReadLine();

            bool needSign = !String.IsNullOrEmpty(thumbprint);

            EntityDescriptorType entityDescriptorType = CreateEntityDescriptor(entityId, metadataAttributes);

            // Serialize EntityDescriptorType instance
            using(StringWriter stringWriter = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = false,
                    Encoding = Encoding.ASCII
                };

                using(XmlWriter responseWriter = XmlWriter.Create(stringWriter, settings))
                {
                    XmlSerializer responseSerializer = new XmlSerializer(entityDescriptorType.GetType());
                    responseSerializer.Serialize(responseWriter, entityDescriptorType);
                }

                try
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(stringWriter.ToString());

                    if (needSign)
                    {
                        SignDoc(thumbprint, entityDescriptorType.ID, ref xmlDocument);
                    }

                    using (StreamWriter sw = new StreamWriter("metadata.xml", false, Encoding.ASCII))
                    {
                        // Use this code to write file without <xml version> tag header
                        sw.Write(xmlDocument.OuterXml);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error happened: " + ex.Message);
                }
            }
        }

        #region Private methods

        /// <summary>
        /// Signs xml document with certificate.
        /// </summary>
        private static void SignDoc(string thumbprint, string assertionTypeId, ref XmlDocument xmlDocument)
        {
            X509Certificate2 x509 = null;

            try
            {
                x509 = X509CertificateHelper.GetCertificateByThumbprint(thumbprint, StoreName.My, StoreLocation.LocalMachine);

                var signedXml = SigningHelper.SignXml(xmlDocument, x509, "ID", assertionTypeId);
                xmlDocument.DocumentElement?.InsertBefore(signedXml.GetXml(),
                    xmlDocument.DocumentElement.ChildNodes[0]);
            }
            catch (SecurityException)
            {
                Console.WriteLine("Information could not be written out for this certificate due to Security Error.");
            }
            catch (CryptographicException)
            {
                Console.WriteLine("Information could not be written out for this certificate due to Cryptographic Error.");
            }
            finally
            {
                if (x509 != null)
                {
                    x509.Reset();
                }
            }
        }

        /// <summary>
        /// Creates body of metadata file.
        /// </summary>
        private static EntityDescriptorType CreateEntityDescriptor(string entityId, Dictionary<string, string> attributes)
        {
            var samlAttributes = new List<AttributeType>();

            foreach (var attribute in attributes)
            {
                samlAttributes.Add(
                    new AttributeType
                    {
                        Name = attribute.Key,
                        NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                        AttributeValue = new object[] { attribute.Value }
                    });
            }

            var descriptor = new IDPSSODescriptorType
            {
                ID = "_" + Guid.NewGuid().ToString(),
                Attribute = samlAttributes.ToArray(),
                protocolSupportEnumeration = new string[] { "urn:oasis:names:tc:SAML:2.0:protocol" },
                KeyDescriptor = new KeyDescriptorType[] { new KeyDescriptorType() },
                SingleSignOnService = new EndpointType[]
                {
                    new EndpointType()
                    { 
                        Binding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect", 
                        Location = entityId
                    } 
                }
            };

            var entityDescriptor = new EntityDescriptorType()
            {
                ID = "_" + Guid.NewGuid().ToString(),
                entityID = entityId,
                Items = new IDPSSODescriptorType[] { descriptor }
            };

            return entityDescriptor;
        }

        #endregion
    }
}
