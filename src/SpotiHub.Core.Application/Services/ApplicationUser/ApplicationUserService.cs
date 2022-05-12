using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Octokit;
using SpotiHub.Core.Application.Options;

namespace SpotiHub.Core.Application.Services.ApplicationUser;

public class ApplicationUserService : IApplicationUserService
{
    private readonly ILogger<ApplicationUserService> _logger;
    private readonly UserManager<Entity.ApplicationUser> _userManager;
    private readonly IGitHubClient _gitHubClient;
    private readonly GitHubOptions _options;

    public ApplicationUserService(ILogger<ApplicationUserService> logger, UserManager<Entity.ApplicationUser> userManager, 
        IGitHubClient gitHubClient, IOptions<GitHubOptions> options)
    {
        _logger = logger;
        _userManager = userManager;
        _gitHubClient = gitHubClient;
        _options = options.Value;
    }

    public string GetLoginUrl(CancellationToken cancellationToken = default)
    {
        return _gitHubClient.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(_options.ClientId)
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
        var id = await Create();
        
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
        
        async Task<string> Create()
        {
            var temp = new Entity.ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = username,
                Email = email,
            };
            var result = await _userManager.CreateAsync(temp);

            if (!result.Succeeded)
            {
                throw new Exception();
            }

            return temp.Id;
        }
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

    private async Task CreateApplicationUserAsync(User gitHubUser, IReadOnlyList<string> scopes, string token)
    {

    }

    private async Task<OauthToken?> GetTokenFromGitHubAsync(string code, string? state, CancellationToken cancellationToken = default)
    {
        var request = new OauthTokenRequest(_options.ClientId, _options.ClientSecret, code);
        var response = await _gitHubClient.Oauth.CreateAccessToken(request);

        return response is not null && string.IsNullOrWhiteSpace(response.Error) 
            ? response 
            : default;
    }
    
    private async Task<User> GetGitHubProfileAsync(string token, CancellationToken cancellationToken = default)
    {
        _gitHubClient.Connection.Credentials = new Credentials(token);

        return await _gitHubClient.User.Current();
    }
}