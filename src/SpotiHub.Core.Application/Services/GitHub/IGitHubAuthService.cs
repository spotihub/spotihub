namespace SpotiHub.Core.Application.Services.GitHub;

public interface IGitHubAuthService
{
    Task<string> GetLoginUrl(CancellationToken cancellationToken = default);
    Task<(string Token, IReadOnlyList<string> Scopes)?> Authorize(string code, CancellationToken cancellationToken = default);
}