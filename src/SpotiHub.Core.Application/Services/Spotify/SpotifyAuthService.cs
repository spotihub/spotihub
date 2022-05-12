using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Incremental.Common.Sourcing.Abstractions.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotiHub.Core.Application.Events.Contracts;
using SpotiHub.Infrastructure.Contract.Services;
using SpotiHub.Infrastructure.Contract.Services.Options;

namespace SpotiHub.Core.Application.Services.Spotify;

public class SpotifyAuthService : ISpotifyAuthService
{
    private readonly ILogger<SpotifyAuthService> _logger;
    private readonly UserManager<Entity.ApplicationUser> _userManager;
    private readonly ISpotifyClientFactory _spotifyClientFactory;
    private readonly SpotifyOptions _options;
    private readonly IEventBus _eventBus;


    public SpotifyAuthService(ILogger<SpotifyAuthService> logger, UserManager<Entity.ApplicationUser> userManager,
        ISpotifyClientFactory spotifyClientFactory, IOptions<SpotifyOptions> options, IEventBus eventBus)
    {
        _logger = logger;
        _userManager = userManager;
        _spotifyClientFactory = spotifyClientFactory;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async Task<string> GetLoginUrl(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var state = new Claim("spotify:integration_state", Guid.NewGuid().ToString());

        await _userManager.AddClaimAsync(user, state);

        var loginRequest = new LoginRequest(_options.RedirectUrl, _options.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new[]
            {
                Scopes.UserReadPrivate,
                Scopes.UserReadRecentlyPlayed,
                Scopes.UserReadRecentlyPlayed
            },
            State = state.Value
        };

        return loginRequest.ToUri().ToString();
    }

    public async Task<Entity.ApplicationUser?> AuthorizeAsync(string code, string state, CancellationToken cancellationToken = default)
    {
        var user = await FindUserFromStateAsync(state);

        var token = await GetSpotifyTokenAsync(code, cancellationToken);
        
        await StoreOrReplaceRefreshTokenAsync(user, token.RefreshToken);

        var profile = await GetSpotifyProfileAsync(user, cancellationToken);

        await _userManager.AddLoginAsync(user, new UserLoginInfo("spotify", profile.Id, profile.DisplayName));
        
        await _userManager.AddClaimAsync(user, new Claim("spotify", profile.Id));
        await _userManager.AddClaimAsync(user, new Claim("spotify:country", profile.Country));
        await _userManager.AddClaimsAsync(user, token.Scope.Split(' ').Select(scope => new Claim("spotify:scope", scope)));

        await _eventBus.Publish(new SpotifyAccountLinked{ UserId = new Guid(user.Id)}, cancellationToken);

        return user;
    }
    
    private async Task<Entity.ApplicationUser> FindUserFromStateAsync(string state)
    {
        var claim = new Claim("spotify:integration_state", state);
        var user = (await _userManager.GetUsersForClaimAsync(claim)).First();
        
        if (user is null)
        {
            throw new Exception();
        }

        await _userManager.RemoveClaimAsync(user, claim);

        return user;
    }

    private async Task<AuthorizationCodeTokenResponse> GetSpotifyTokenAsync(string code, CancellationToken cancellationToken)
    {
        var authClient = await _spotifyClientFactory.GetAuthClientAsync(cancellationToken);
        
        var request = new AuthorizationCodeTokenRequest(_options.ClientId, _options.ClientSecret, code, _options.RedirectUrl);
        
        return await authClient.RequestToken(request);
    }

    private async Task<PrivateUser> GetSpotifyProfileAsync(Entity.ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var client = await _spotifyClientFactory.GetClientAsync(user.Id, cancellationToken);

        return await client.UserProfile.Current();
    }

    private async Task StoreOrReplaceRefreshTokenAsync(Entity.ApplicationUser user, string token)
    {
        var claim = new Claim("spotify:refresh_token", token);

        await _userManager.AddClaimAsync(user, claim);
    }
}