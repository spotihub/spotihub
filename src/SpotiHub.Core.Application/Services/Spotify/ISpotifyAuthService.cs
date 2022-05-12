using System.Threading;
using System.Threading.Tasks;

namespace SpotiHub.Core.Application.Services.Spotify;

public interface ISpotifyAuthService
{
    Task<string> GetLoginUrl(string userId, CancellationToken cancellationToken = default);
    Task<Entity.ApplicationUser?> AuthorizeAsync(string code, string state, CancellationToken cancellationToken = default);
}