using System.Threading;
using System.Threading.Tasks;
using Incremental.Common.Sourcing.Abstractions.Events;
using MassTransit;
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

    public override async Task Consume(ConsumeContext<SpotifyAccountLinked> context)
    {
        await _scheduler.Schedule(context.Message.UserId.ToString(), context.CancellationToken);
    }
}