using System;
using Incremental.Common.Sourcing.Abstractions.Events;

namespace SpotiHub.Core.Application.Events.Contracts;

public record SpotifyAccountLinked : Event
{
    public Guid UserId { get; init; }
}