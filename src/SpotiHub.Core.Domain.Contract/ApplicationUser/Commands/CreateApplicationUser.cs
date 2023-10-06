using Incremental.Common.Sourcing.Abstractions.Commands;

namespace SpotiHub.Core.Domain.Contract.ApplicationUser.Commands;

public record CreateApplicationUser : Command
{
    public Guid ApplicationUserId { get; init; }
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
}