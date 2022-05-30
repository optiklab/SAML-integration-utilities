using System;

namespace SamlIntegration.Example
{
    public interface IIntegrationClient
    {
        Uri GetRedirectUrl();
    }
}
