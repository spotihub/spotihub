using Incremental.Common.Authentication.Jwt;
using Microsoft.AspNetCore.Mvc;
using SpotiHub.Core.Application.Services.ApplicationUser;
using SpotiHub.Core.Application.Services.GitHub;

namespace SpotiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGitHubAuthService _gitHubAuthService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public IntegrationController(IApplicationUserService applicationUserService, IGitHubAuthService gitHubAuthService,
        ITokenService tokenService, IConfiguration configuration)
    {
        _applicationUserService = applicationUserService;
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

    [HttpPost("spotify", Name = nameof(LinkSpotify))]
    public async Task<IActionResult> LinkSpotify(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}