using Incremental.Common.Sourcing.Abstractions.Events;

namespace SpotiHub.Core.Domain.Contract.ApplicationUser.Events;

public record OptionsUpdated : Event
{
    public Guid ApplicationUserId { get; init; }
}