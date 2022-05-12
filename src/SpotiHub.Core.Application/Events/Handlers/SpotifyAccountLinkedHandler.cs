using System.Threading;
using System.Threading.Tasks;
using Incremental.Common.Sourcing.Abstractions.Events;
using SpotiHub.Core.Application.Events.Contracts;
using SpotiHub.Core.Application.Services.Scheduler;

namespace SpotiHub.Core.Application.Events.Handlers;

public class SpotifyAccountLinkedHandler : EventHandler<SpotifyAccountLinked>
{
    private readonly ISchedulerService _scheduler;

    public SpotifyAccountLinkedHandler(ISchedulerService scheduler)
    {
        _scheduler = scheduler;
    }

    public override async Task Handle(SpotifyAccountLinked @event, CancellationToken cancellationToken)
    {
        await _scheduler.Schedule(@event.UserId.ToString(), cancellationToken);
    }
}