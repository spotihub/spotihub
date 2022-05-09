namespace SpotiHub.Core.Application.Services.ApplicationUser;

public interface IApplicationUserService
{
    public Task<Entity.ApplicationUser?> GetOrCreate(string token, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);
}