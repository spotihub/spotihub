using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotiHub.Core.Domain.Contract.Services;
using SpotiHub.Core.Domain.Contract.Services.Options;
using SpotiHub.Core.Entity;

namespace SpotiHub.Core.Domain.Services;

public class SpotifyClientFactory : ISpotifyClientFactory
{
    private readonly ILogger<SpotifyClientFactory> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SpotifyClientConfig _defaultClientConfig;
    private readonly SpotifyOptions _options;
    private readonly IDistributedCache _distributedCache;

    public SpotifyClientFactory(ILogger<SpotifyClientFactory> logger, UserManager<ApplicationUser> userManager, 
        SpotifyClientConfig defaultClientConfig, IOptions<SpotifyOptions> options, IDistributedCache distributedCache)
    {
        _logger = logger;
        _userManager = userManager;
        _defaultClientConfig = defaultClientConfig;
        _distributedCache = distributedCache;
        _options = options.Value;
    }

    public Task<IOAuthClient> GetAuthClientAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IOAuthClient>(new OAuthClient(_defaultClientConfig));
    }

    public async Task<ISpotifyClient> GetClientAsync(string userId, CancellationToken cancellationToken = default)
    {
        var auth = await GetCachedAuthOrDefaultAsync(userId, cancellationToken);

        if (auth is null)
        {
            auth = await GetNewAuthAsync(userId, cancellationToken);
        }

        return new SpotifyClient(_defaultClientConfig.WithToken(auth.AccessToken));
    }

    private async Task<AuthorizationCodeRefreshResponse?> GetCachedAuthOrDefaultAsync(string user, CancellationToken cancellationToken = default)
    { 
        var cachedToken = await _distributedCache.GetStringAsync(user, cancellationToken);

        if (cachedToken is null)
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<AuthorizationCodeRefreshResponse>(cachedToken);
    }
    
    private async Task CacheAndStoreTokensAsync(ApplicationUser user, AuthorizationCodeRefreshResponse token)
    {
        var serializedToken = JsonSerializer.Serialize(token);
        var cacheEntryOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(token.ExpiresIn).Subtract(TimeSpan.FromMinutes(5)));
        
        await _distributedCache.SetStringAsync(user.Id, serializedToken, cacheEntryOptions);

        await _userManager.AddLoginAsync(user, new UserLoginInfo("spotify:refresh_token", token.RefreshToken, "spotify:refresh_token"));
    }

    private async Task<AuthorizationCodeRefreshResponse> GetNewAuthAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var authClient = await GetAuthClientAsync(cancellationToken);

        var token = await authClient.RequestToken(await BuildRefreshRequest(user, cancellationToken));

        await CacheAndStoreTokensAsync(user, token);

        return token;
    }

    private async Task<AuthorizationCodeRefreshRequest> BuildRefreshRequest(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var logins = await _userManager.GetLoginsAsync(user);

        var refreshToken = logins.First(login => login.LoginProvider == "spotify:refresh_token").ProviderKey;

        await _userManager.RemoveLoginAsync(user, "spotify:refresh_token", refreshToken);

        return new AuthorizationCodeRefreshRequest(_options.ClientId, _options.ClientSecret, refreshToken);
    }
}