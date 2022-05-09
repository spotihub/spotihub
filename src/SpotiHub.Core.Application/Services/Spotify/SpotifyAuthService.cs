using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotiHub.Core.Application.Options;

namespace SpotiHub.Core.Application.Services.Spotify;

public class SpotifyAuthService : ISpotifyAuthService
{
    private readonly ILogger<SpotifyAuthService> _logger;
    private readonly ISpotifyClient _spotifyClient;
    private readonly SpotifyOptions _options;
    private readonly SpotifyClientConfig _spotifyClientConfig;
    private readonly UserManager<Entity.ApplicationUser> _userManager;

    public SpotifyAuthService(ILogger<SpotifyAuthService> logger, ISpotifyClient spotifyClient, IOptions<SpotifyOptions> options,
        SpotifyClientConfig spotifyClientConfig, UserManager<Entity.ApplicationUser> userManager)
    {
        _logger = logger;
        _spotifyClient = spotifyClient;
        _spotifyClientConfig = spotifyClientConfig;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task<string> GetLoginUrl(string user, CancellationToken cancellationToken = default)
    {
        var state = Guid.NewGuid().ToString();

        await _userManager.AddLoginAsync(await _userManager.FindByIdAsync(user), new UserLoginInfo("spotify", state, "Transition"));
        
        var loginRequest = new LoginRequest(_options.RedirectUrl, _options.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new[]
            {
                Scopes.UserReadPrivate,
                Scopes.UserReadRecentlyPlayed,
                Scopes.UserReadRecentlyPlayed
            },
            State = state
        };

        return loginRequest.ToUri().ToString();
    }

    public async Task Authorize(string code, string state,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByLoginAsync("spotify", state);

        if (user is null)
        {
            return;
        }
        
        var response = await new OAuthClient(_spotifyClientConfig)
            .RequestToken(new AuthorizationCodeTokenRequest(
                _options.ClientId,
                _options.ClientSecret,
                code,
                _options.RedirectUrl));

        await _userManager.RemoveLoginAsync(user, "spotify", state);

        await _userManager.AddLoginAsync(user, new UserLoginInfo("spotify:token", response.AccessToken, "spotify:token"));
        await _userManager.AddLoginAsync(user, new UserLoginInfo("spotify:refresh_token", response.RefreshToken, "spotify:refresh_token"));

        await _userManager.AddClaimsAsync(user, response.Scope.Split(',').Select(scope => new Claim("spotify:scope", scope)));
    }
}