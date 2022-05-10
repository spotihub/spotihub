using Incremental.Common.Authentication;
using Incremental.Common.Authentication.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotiHub.Core.Application.Services.ApplicationUser;
using SpotiHub.Core.Application.Services.GitHub;
using SpotiHub.Core.Application.Services.Spotify;

namespace SpotiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly ISpotifyAuthService _spotifyAuthService;
    private readonly IGitHubAuthService _gitHubAuthService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public IntegrationController(IApplicationUserService applicationUserService, ISpotifyAuthService spotifyAuthService, IGitHubAuthService gitHubAuthService, ITokenService tokenService, IConfiguration configuration)
    {
        _applicationUserService = applicationUserService;
        _spotifyAuthService = spotifyAuthService;
        _gitHubAuthService = gitHubAuthService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [HttpGet("github", Name = nameof(GitHub))]
    public async Task<IActionResult> GitHub(CancellationToken cancellationToken)
    {
        var destination = await _gitHubAuthService.GetLoginUrl(cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return Problem();
        }

        return Redirect(destination);
    }

    [HttpGet("github/authorize", Name = nameof(GitHubAuthorize))]
    public async Task<ActionResult> GitHubAuthorize([FromQuery] string code, CancellationToken cancellationToken)
    {
        var token = await _gitHubAuthService.Authorize(code, cancellationToken);

        if (!token.HasValue)
        {
            return Problem();
        }

        var user = await _applicationUserService.GetOrCreate(token.Value.Token, token.Value.Scopes, cancellationToken);

        if (user is null)
        {
            return Problem();
        }

        var tokenInfo = await _tokenService.GenerateTokenAsync(user.Id);

        return Redirect($"{_configuration["SPA_BASE_URI"]}/login?token={tokenInfo.Token}&refreshToken={tokenInfo.RefreshToken}");
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
        await _spotifyAuthService.Authorize(code, state, cancellationToken);
        
        return Redirect($"{_configuration["SPA_BASE_URI"]}");
    }
}