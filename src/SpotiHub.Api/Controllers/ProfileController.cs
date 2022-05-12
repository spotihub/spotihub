using System.Security.Claims;
using Incremental.Common.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpotiHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            Id = User.GetId(),
            Username = User.GetUsername(),
            Integrations = new
            {
                Spotify = User.FindFirstValue("spotify") ?? default
            }
        });
    }
}