using Incremental.Common.Sourcing.Abstractions.Events;

namespace SpotiHub.Core.Domain.Contract.ApplicationUser.Events;

public record ApplicationUserCreated : Event
{
    public Guid ApplicationUserId { get; init; }
}