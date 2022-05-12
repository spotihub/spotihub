using Incremental.Common.Authentication;
using Incremental.Common.Authentication.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotiHub.Core.Application.Services.Spotify;

namespace SpotiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly ISpotifyAuthService _spotifyAuthService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public IntegrationController(ISpotifyAuthService spotifyAuthService, ITokenService tokenService, IConfiguration configuration)
    {
        _spotifyAuthService = spotifyAuthService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet("spotify", Name = nameof(Spotify))]
    public async Task<IActionResult> Spotify(CancellationToken cancellationToken)
    {
        var destination = await _spotifyAuthService.GetLoginUrl(User.GetId(), cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return Problem();
        }

        return Ok(destination);
    }
    
    [HttpGet("spotify/authorize", Name = nameof(SpotifyAuthorize))]
    public async Task<IActionResult> SpotifyAuthorize([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
    {
        await _spotifyAuthService.AuthorizeAsync(code, state, cancellationToken);
        
        return Redirect($"{_configuration["SPA_BASE_URI"]}/login/refresh");
    }
}