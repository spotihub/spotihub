using Incremental.Common.Sourcing.Abstractions.Commands;

namespace SpotiHub.Core.Domain.Contract.ApplicationUser.Commands;

public record UpdateOptions : Command
{
    public Guid ApplicationUserId { get; init; }
    public bool? Enabled { get; init; }
    public bool? LimitedAvailability { get; init; }
    public bool? GenreEmojis { get; init; }
    public TimeSpan? ClearAfter { get; init; }
}