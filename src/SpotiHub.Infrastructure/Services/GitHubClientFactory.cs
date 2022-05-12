using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using SpotiHub.Core.Entity;
using SpotiHub.Infrastructure.Contract.Services;
using SpotiHub.Infrastructure.Contract.Services.Options;
using Connection = Octokit.GraphQL.Connection;
using ProductHeaderValue = Octokit.GraphQL.ProductHeaderValue;

namespace SpotiHub.Infrastructure.Services;

public class GitHubClientFactory : IGitHubClientFactory
{
    private readonly ILogger<GitHubClientFactory> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GitHubOptions _options;
    
    public GitHubClientFactory(ILogger<GitHubClientFactory> logger, UserManager<ApplicationUser> userManager, IOptions<GitHubOptions> options)
    {
        _logger = logger;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task<Connection> GetConnectionAsync(string user, CancellationToken cancellationToken = default)
    {
        var token = await GetUserTokenAsync(user);

        return new Connection(new ProductHeaderValue(_options.Name), token);
    }

    public async Task<GitHubClient> GetGitHubClientAsync(string? user = default, CancellationToken cancellationToken = default)
    {
        var client = new GitHubClient(new Octokit.ProductHeaderValue(_options.Name));

        if (user is not null)
        {
            var token = await GetUserTokenAsync(user);

            client.Credentials = new Credentials(token);
        }

        return client;
    }
    
    private async Task<string> GetUserTokenAsync(string user)
    {
        var claims = await _userManager.GetClaimsAsync(await _userManager.FindByIdAsync(user));

        var token = claims.First(claim => claim.Type == "github:token");

        if (token is null)
        {
            throw new Exception();
        }

        return token.Value;
    }
}