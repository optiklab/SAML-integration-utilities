using Microsoft.Extensions.Logging;
using SamlIntegration.Utilities.Schemas;
using SamlIntegration.Utilities.UserData;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SamlIntegration.Utilities.Helpers
{
    public class SamlAssertionAlgorithms
    {
        private const string IdPrefix = "_";
        private const string XsiSchema = @"http://www.w3.org/2001/XMLSchema-instance";
        private const string XsdSchema = @"http://www.w3.org/2001/XMLSchema";

        private readonly ILogger<SamlAssertionAlgorithms> _logger;

        protected SamlAssertionAlgorithms(ILogger<SamlAssertionAlgorithms> logger)
        {
            _logger = logger;
        }

        public static AssertionType Create(SamlIntegrationSettings settings, IUserDataRepository userData)
        {
            var items = new List<AttributeType>();
            foreach (var attribute in settings.Attributes)
            {
                var attr = new AttributeType
                {
                    Name = attribute.Key,
                    NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                    AttributeValue = new object[] { attribute.Value }
                };

                items.Add(attr);
            }

            var conditions = new List<ConditionAbstractType>();
            conditions.Add(new OneTimeUseType());
            conditions.Add(new AudienceRestrictionType
            {
                Audience = new string[] { settings.Audience }
            });

            // Create assertion instance
            string assertionId = IdPrefix + Guid.NewGuid().ToString();
            DateTime issueTime = DateTime.UtcNow;

            AssertionType assertion = new AssertionType
            {
                ID = assertionId,
                IssueInstant = issueTime,
                Version = "2.0",
                Issuer = new NameIDType
                {
                    Value = settings.Issuer
                },
                Subject = new SubjectType
                {
                    Items = new object[]
                    {
                        new NameIDType
                        {
                            Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:emailAddress",
                            Value = userData.GetUserEmail()
                        },
                        new SubjectConfirmationType
                        {
                            Method = "urn:oasis:names:tc:SAML:2.0:cm:bearer",
                            SubjectConfirmationData = new SubjectConfirmationDataType
                            {
                                NotOnOrAfter = issueTime.AddMinutes(3),
                                NotOnOrAfterSpecified = true,
                                Recipient = settings.Recipient
                            }
                        }
                    }
                },
                Conditions = new ConditionsType
                {
                    NotBefore = issueTime,
                    NotBeforeSpecified = true,
                    NotOnOrAfter = issueTime.AddMinutes(3),
                    NotOnOrAfterSpecified = true,
                    Items = conditions.ToArray()
                },
                Items = new StatementAbstractType[]
                {
                    new AttributeStatementType
                    {
                        // ReSharper disable once CoVariantArrayConversion
                        Items = items.ToArray()
                    },
                    new AuthnStatementType
                    {
                        AuthnInstant = issueTime,
                        SessionIndex = assertionId,
                        AuthnContext = new AuthnContextType
                        {
                            ItemsElementName = new [] { ItemsChoiceType5.AuthnContextClassRef },
                            Items = new object[] { "urn:federation:authentication:windows" }
                        }
                    }
                }
            };

            return assertion;
        }

        public bool Sign(string thumbprint, string responseId, ref XmlElement xmlAssertion)
        {
            X509Certificate2 x509 = null;
            bool result = false;

            try
            {
                x509 = X509CertificateHelper.GetCertificateByThumbprint(thumbprint, StoreName.My, StoreLocation.LocalMachine);

                if (x509 == null)
                {

                    SamlSignedXml samlSignedElement = SigningHelper.SignXml(xmlAssertion, x509, "ID", responseId);

                    // Get the XML representation of the signature and save it to an XmlElement object. 
                    XmlElement xmlDigitalSignature = samlSignedElement.GetXml();

                    // Put the sign as the first child of main Request tag.
                    xmlAssertion?.InsertAfter(xmlDigitalSignature, xmlAssertion.ChildNodes[0]);

                    result = true;
                }
                else
                {
                    _logger.LogDebug("X509 certificate not found!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Exception: " + ex.Message + Environment.NewLine +
                                 (ex.InnerException != null ? "InnerException: " + ex.InnerException.Message : string.Empty)
                                 + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                x509?.Reset();
            }

            return result;
        }

        public bool Encrypt(string thumbprint, ref XmlDocument xmlDocument)
        {
            X509Certificate2 x509 = null;
            bool result = false;

            try
            {
                x509 = X509CertificateHelper.GetCertificateByThumbprint(thumbprint, StoreName.My, StoreLocation.LocalMachine);

                if (x509 == null)
                {
                    XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                    namespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                    namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                    namespaceManager.AddNamespace("xsi", XsiSchema);
                    namespaceManager.AddNamespace("xsd", XsdSchema);

                    XmlElement xmlAssertionSource =
                        (XmlElement) xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion", namespaceManager);

                    EncryptedXml eXml = new EncryptedXml();

                    var encryptedData = eXml.Encrypt(xmlAssertionSource, x509);

                    XmlDocument encryptedAssertion = new XmlDocument();

                    // Add namespaces
                    XmlDeclaration xmlDeclaration = encryptedAssertion.CreateXmlDeclaration("1.0", "UTF-8", null);
                    XmlElement encryptedRoot = encryptedAssertion.DocumentElement;
                    encryptedAssertion.InsertBefore(xmlDeclaration, encryptedRoot);

                    // Form Assertion element
                    XmlElement encryptedAssertionElement = encryptedAssertion.CreateElement("saml",
                        "EncryptedAssertion", "urn:oasis:names:tc:SAML:2.0:assertion");
                    encryptedAssertion.AppendChild(encryptedAssertionElement);

                    // Add encrypted content
                    var encryptedDataNode = encryptedAssertion.ImportNode(encryptedData.GetXml(), true);
                    encryptedAssertionElement.AppendChild(encryptedDataNode);

                    // Form a document
                    var root = xmlDocument.DocumentElement;
                    var node = root.OwnerDocument.ImportNode(encryptedAssertionElement, true);
                    root.RemoveChild(xmlAssertionSource ?? throw new InvalidOperationException());
                    root.AppendChild(node);

                    result = true;
                }
                else
                {
                    _logger.LogDebug("X509 certificate not found!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Exception: " + ex.Message + Environment.NewLine +
                                 (ex.InnerException != null ? "InnerException: " + ex.InnerException.Message : string.Empty)
                                 + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                x509?.Reset();
            }

            return result;
        }
    }
}
