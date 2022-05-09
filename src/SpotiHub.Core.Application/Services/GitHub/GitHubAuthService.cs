using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using SpotiHub.Core.Application.Options;

namespace SpotiHub.Core.Application.Services.GitHub;

public class GitHubAuthService : IGitHubAuthService
{
    private readonly ILogger<GitHubAuthService> _logger;
    private readonly GitHubOptions _options;
    private readonly IGitHubClient _client;

    public GitHubAuthService(ILogger<GitHubAuthService> logger, IOptions<GitHubOptions> options, IGitHubClient client)
    {
        _logger = logger;
        _options = options.Value;
        _client = client;
    }

    public Task<string> GetLoginUrl(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(_options.ClientId)
        {
            Scopes = { "user" }
        }).ToString());
    }

    public async Task<(string Token, IReadOnlyList<string> Scopes)?> Authorize(string code, CancellationToken cancellationToken = default)
    {
        var request = new OauthTokenRequest(_options.ClientId, _options.ClientSecret, code);
        var response = await _client.Oauth.CreateAccessToken(request);

        if (response is null || !string.IsNullOrWhiteSpace(response.Error))
        {
            return default;
        }
        
        return (response.AccessToken, response.Scope);
    }
}