namespace SpotiHub.Core.Application.Services.ApplicationUser;

public interface IApplicationUserService
{
    string GetLoginUrl(CancellationToken cancellationToken = default);
    public Task<Entity.ApplicationUser?> AuthorizeAsync(string token, string? state = default, CancellationToken cancellationToken = default);
}