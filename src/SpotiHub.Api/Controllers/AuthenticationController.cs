using System.Text;
using Incremental.Common.Authentication.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SpotiHub.Core.Application.Services.ApplicationUser;

namespace SpotiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IApplicationUserService applicationUserService, ITokenService tokenService, IConfiguration configuration)
    {
        _applicationUserService = applicationUserService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("login")]
    public async Task<IActionResult> Login(CancellationToken cancellationToken)
    {
        var destination = await _applicationUserService.GetLoginUrl(cancellationToken);

        return Redirect(destination);
    }

    [HttpGet("authorize", Name = nameof(GitHubAuthorize))]
    public async Task<ActionResult> GitHubAuthorize([FromQuery] string code, [FromQuery] string? state,  CancellationToken cancellationToken)
    {
        var user = await _applicationUserService.AuthorizeAsync(code, state, cancellationToken);
        
        if (user is null)
        {
            return Problem();
        }

        var tokenInfo = await _tokenService.GenerateTokenAsync(user.Id);

        return Redirect($"{_configuration["SPA_BASE_URI"]}/login?token={tokenInfo.Token}&refreshToken={tokenInfo.RefreshToken}");
    }
    
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> Refresh([FromBody] JwtRefresh model, CancellationToken cancellationToken)
    {
        string? token;
        
        using var reader = new StreamReader (Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync ();
        
        try
        {
            token =  Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
        }
        catch (Exception)
        {
            return Problem();
        }

        var refreshedToken = await _tokenService.RefreshTokenAsync(new JwtToken
        {
            Token = token,
            RefreshToken = model.RefreshToken
        });

        if (refreshedToken is null)
        {
            ModelState.AddModelError("RefreshError", "Invalid refresh attempt. Check jwt token or refresh token.");
            return BadRequest(ModelState);
        }
        
        return Ok(refreshedToken);
    }

    public record JwtRefresh
    {
        public Guid RefreshToken { get; init; }
    }
}