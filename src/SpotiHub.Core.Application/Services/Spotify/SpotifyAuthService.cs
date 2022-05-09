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

    public SpotifyAuthService(ILogger<SpotifyAuthService> logger, ISpotifyClient spotifyClient, IOptions<SpotifyOptions> options)
    {
        _logger = logger;
        _spotifyClient = spotifyClient;
        _options = options.Value;
    }

    public Task<string> GetLoginUrl(CancellationToken cancellationToken = default)
    {
        var loginRequest = new LoginRequest(_options.RedirectUrl, _options.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new[]
            {
                Scopes.UserReadPrivate,
                Scopes.UserReadRecentlyPlayed,
                Scopes.UserReadRecentlyPlayed
            }
        };

        return Task.FromResult(loginRequest.ToUri().ToString());
    }
}