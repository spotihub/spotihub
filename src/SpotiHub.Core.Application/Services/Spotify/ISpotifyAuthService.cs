namespace SpotiHub.Core.Application.Services.Spotify;

public interface ISpotifyAuthService
{
    Task<string> GetLoginUrl(CancellationToken cancellationToken = default);
}