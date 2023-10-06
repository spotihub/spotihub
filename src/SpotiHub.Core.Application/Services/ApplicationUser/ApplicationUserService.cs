using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Incremental.Common.Sourcing.Abstractions.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Octokit;
using SpotiHub.Core.Domain.Contract.ApplicationUser.Commands;
using SpotiHub.Infrastructure.Contract.Services;
using SpotiHub.Infrastructure.Contract.Services.Options;

namespace SpotiHub.Core.Application.Services.ApplicationUser;

public class ApplicationUserService : IApplicationUserService
{
    private readonly ILogger<ApplicationUserService> _logger;
    private readonly UserManager<Entity.ApplicationUser> _userManager;
    private readonly IGitHubClientFactory _gitHubClientFactory;
    private readonly GitHubOptions _options;
    private readonly ICommandBus _commandBus;

    public ApplicationUserService(ILogger<ApplicationUserService> logger, UserManager<Entity.ApplicationUser> userManager, 
        IGitHubClientFactory gitHubClientFactory, IOptions<GitHubOptions> options, ICommandBus commandBus)
    {
        _logger = logger;
        _userManager = userManager;
        _gitHubClientFactory = gitHubClientFactory;
        _commandBus = commandBus;
        _options = options.Value;
    }

    public async Task<string> GetLoginUrl(CancellationToken cancellationToken = default)
    {
        var client = await _gitHubClientFactory.GetGitHubClientAsync();
        
        return client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(_options.ClientId)
        {
            Scopes = { "user" }
        }).ToString();
    }

    public async Task<Entity.ApplicationUser?> AuthorizeAsync(string code, string? state = default, CancellationToken cancellationToken = default)
    {
        var token = await GetTokenFromGitHubAsync(code, state, cancellationToken);

        if (token is null)
        {
            return default;
        }
        
        var profile = await GetGitHubProfileAsync(token.AccessToken, cancellationToken);

        var user = await _userManager.FindByLoginAsync("github", profile.Id.ToString());
        
        if (user is null)
        {
            user = await CreateUserAsync(profile.Id, profile.Login, profile.Email);
        }
        
        await StoreOrReplaceGitHubScopesAsync(user, token.Scope);
        await StoreOrReplaceGitHubTokenAsync(user, token.AccessToken);

        return user;
    }

    private async Task<Entity.ApplicationUser> CreateUserAsync(int externalId, string username, string email)
    {
        var id = await Create(username, email);
        
        var user = await _userManager.FindByIdAsync(id);
        
        await _userManager.AddLoginAsync(user, new UserLoginInfo("github", externalId.ToString(), username));
        
        await _userManager.AddClaimsAsync(user, new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("github:id", externalId.ToString())
        });
        
        return user;
    }

    private async Task<string> Create(string username, string email)
    {
        var id = Guid.NewGuid();

        await _commandBus.Send(new CreateApplicationUser
        {
            ApplicationUserId = id,
            Username = username,
            Email = email
        });
        
        return id.ToString();
    }

    private async Task StoreOrReplaceGitHubScopesAsync(Entity.ApplicationUser user, IEnumerable<string> scopes)
    {
        await _userManager.RemoveClaimsAsync(user, (await _userManager.GetClaimsAsync(user)).Where(claim => claim.Type == "github:scope"));
        
        await _userManager.AddClaimsAsync(user, scopes.Select(scope => new Claim("github:scope", scope)));
    }
    
    private async Task StoreOrReplaceGitHubTokenAsync(Entity.ApplicationUser user, string token)
    {
        await _userManager.RemoveClaimsAsync(user, (await _userManager.GetClaimsAsync(user)).Where(claim => claim.Type == "github:token"));

        await _userManager.AddClaimAsync(user, new Claim("github:token", token));
    }
    
    private async Task<OauthToken?> GetTokenFromGitHubAsync(string code, string? state, CancellationToken cancellationToken = default)
    {
        var client = await _gitHubClientFactory.GetGitHubClientAsync(cancellationToken: cancellationToken);

        var request = new OauthTokenRequest(_options.ClientId, _options.ClientSecret, code);
        var response = await client.Oauth.CreateAccessToken(request);

        return response is not null && string.IsNullOrWhiteSpace(response.Error) 
            ? response 
            : default;
    }
    
    private async Task<User> GetGitHubProfileAsync(string token, CancellationToken cancellationToken = default)
    {
        var client = await _gitHubClientFactory.GetGitHubClientAsync(cancellationToken: cancellationToken);

        client.Connection.Credentials = new Credentials(token);

        return await client.User.Current();
    }
}