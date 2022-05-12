using Octokit;
using Connection = Octokit.GraphQL.Connection;

namespace SpotiHub.Infrastructure.Contract.Services;

public interface IGitHubClientFactory
{
    Task<Connection> GetConnectionAsync(string user, CancellationToken cancellationToken = default);

    Task<GitHubClient> GetGitHubClientAsync(string? user, CancellationToken cancellationToken = default);
}