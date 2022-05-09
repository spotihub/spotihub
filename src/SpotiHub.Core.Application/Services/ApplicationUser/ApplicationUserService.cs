using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Octokit;

namespace SpotiHub.Core.Application.Services.ApplicationUser;

public class ApplicationUserService : IApplicationUserService
{
    private readonly ILogger<ApplicationUserService> _logger;
    private readonly UserManager<Entity.ApplicationUser> _userManager;
    private readonly IGitHubClient _gitHubClient;

    public ApplicationUserService(ILogger<ApplicationUserService> logger, UserManager<Entity.ApplicationUser> userManager, IGitHubClient gitHubClient)
    {
        _logger = logger;
        _userManager = userManager;
        _gitHubClient = gitHubClient;
    }

    public async Task<Entity.ApplicationUser?> GetOrCreate(string token, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        var gitHubUser = await GetGitHubUser(token, cancellationToken);

        var applicationUser = await FindByLoginAsync();
        
        if (applicationUser is null)
        {
            await Create(gitHubUser, scopes);
        }
        
        return await FindByLoginAsync();

        async Task<Entity.ApplicationUser?> FindByLoginAsync()
        {
            return await _userManager.FindByLoginAsync("github", gitHubUser.Id.ToString());
        }
    }

    private async Task Create(User gitHubUser, IReadOnlyList<string> scopes)
    {
        var id = Guid.NewGuid().ToString();

        var user = new Entity.ApplicationUser
        {
            Id = id,
            UserName = gitHubUser.Login,
            Email = gitHubUser.Email,
        };
        
        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception();
        }
        
        var userFound = await _userManager.FindByIdAsync(id);

        await _userManager.AddLoginAsync(userFound, new UserLoginInfo("github", gitHubUser.Id.ToString(), gitHubUser.Login));

        await _userManager.AddClaimsAsync(user, new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
        });
        
        await _userManager.AddClaimsAsync(userFound, scopes.Select(scope => new Claim("github:scope", scope)));
    }

    private async Task<User> GetGitHubUser(string token, CancellationToken cancellationToken = default)
    {
        _gitHubClient.Connection.Credentials = new Credentials(token);

        return await _gitHubClient.User.Current();
    }
}