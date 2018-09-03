using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "Premium")]
        public IActionResult Get()
        {
            return new JsonResult("premium");
        }

        [HttpPost]
        [Authorize(Policy = "Normal")]
        public IActionResult Post()
        {
            return new JsonResult("normal");
        }

    }
}