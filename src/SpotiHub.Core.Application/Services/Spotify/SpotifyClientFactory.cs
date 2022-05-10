using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotiHub.Core.Application.Options;

namespace SpotiHub.Core.Application.Services.Spotify;

public class SpotifyClientFactory : ISpotifyClientFactory
{
    private readonly ILogger<SpotifyClientFactory> _logger;
    private readonly UserManager<Entity.ApplicationUser> _userManager;
    private readonly SpotifyClientConfig _defaultClientConfig;
    private readonly SpotifyOptions _options;

    public SpotifyClientFactory(ILogger<SpotifyClientFactory> logger, UserManager<Entity.ApplicationUser> userManager, 
        SpotifyClientConfig defaultClientConfig, IOptions<SpotifyOptions> options)
    {
        _logger = logger;
        _userManager = userManager;
        _defaultClientConfig = defaultClientConfig;
        _options = options.Value;
    }

    public Task<IOAuthClient> GetAuthClientAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IOAuthClient>(new OAuthClient(_defaultClientConfig));
    }

    public async Task<ISpotifyClient> GetClientAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        var authClient = await GetAuthClientAsync(cancellationToken);

        var token = await authClient.RequestToken(await BuildRefreshRequest(user));

        return new SpotifyClient(_defaultClientConfig.WithToken(token.AccessToken));
    }

    private async Task<AuthorizationCodeRefreshRequest> BuildRefreshRequest(Entity.ApplicationUser user)
    {
        var logins = await _userManager.GetLoginsAsync(user);

        var refreshToken = logins.First(login => login.LoginProvider == "spotify:refresh_token").ProviderKey;

        return new AuthorizationCodeRefreshRequest(_options.ClientId, _options.ClientSecret, refreshToken);
    }
}