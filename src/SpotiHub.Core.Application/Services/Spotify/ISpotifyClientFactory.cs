using SpotifyAPI.Web;

namespace SpotiHub.Core.Application.Services.Spotify;

public interface ISpotifyClientFactory
{
    public Task<IOAuthClient> GetAuthClientAsync(CancellationToken cancellationToken = default);
    public Task<ISpotifyClient> GetClientAsync(string userId, CancellationToken cancellationToken = default);
}