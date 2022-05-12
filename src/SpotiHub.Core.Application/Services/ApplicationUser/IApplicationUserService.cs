using System.Threading;
using System.Threading.Tasks;

namespace SpotiHub.Core.Application.Services.ApplicationUser;

public interface IApplicationUserService
{
    Task<string> GetLoginUrl(CancellationToken cancellationToken = default);
    public Task<Entity.ApplicationUser?> AuthorizeAsync(string token, string? state = default, CancellationToken cancellationToken = default);
}