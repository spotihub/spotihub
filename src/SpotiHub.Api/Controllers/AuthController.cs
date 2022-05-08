using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

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