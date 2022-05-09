namespace SpotiHub.Core.Application.Services.Spotify;

public interface ISpotifyAuthService
{
    Task<string> GetLoginUrl(string user, CancellationToken cancellationToken = default);
    Task Authorize(string code, string state, CancellationToken cancellationToken = default);
}