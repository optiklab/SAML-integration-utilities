using Microsoft.AspNetCore.Mvc;
using SamlIntegration.Example.Command;

namespace SamlIntegration.Example.Controllers
{
    [Route("[controller]")]
    //TODO [Authorize(AuthenticationSchemes = ...)]
    public class IntegrationController : Controller
    {
        private IIntegrationClient _client;

        public IntegrationController(IIntegrationClient client)
        {
            _client = client;
        }

        /// <summary>
        /// This method executes a command to authorize with third-party and redirect to the authorized link
        /// </summary>
        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] AuthorizeThirdParty command)
        {
            if (HttpContext.User.Identity.Name == null)
            {
                return Unauthorized();
            }

            return Accepted(_client.GetRedirectUrl());
        }
    }
}