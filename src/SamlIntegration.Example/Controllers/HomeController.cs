using Microsoft.AspNetCore.Mvc;

namespace SamlIntegration.Example.Controllers
{
    /// <summary>
    /// The goal is to show that app is up and running.
    /// </summary>
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Get() => Content("Hello from Example API!");
    }
}