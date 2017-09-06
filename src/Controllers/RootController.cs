using Microsoft.AspNetCore.Mvc;

namespace HappyTokenApi.Controllers
{
    [Route("ping")]
    public class RootController : Controller
    {
        public IActionResult Index()
        {
            return Ok("pong!");
        }
    }
}