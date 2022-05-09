using Microsoft.AspNetCore.Mvc;

namespace SpotiHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public async Task<IActionResult> SignIn()
        {
            // var loginRequest = LoginRequest(new Uri())

            await Task.Delay(10);
            return Ok();
        }
    }
}