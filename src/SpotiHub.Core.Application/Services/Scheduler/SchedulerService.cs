using System.Threading;
using System.Threading.Tasks;
using Quartz;
using SpotiHub.Core.Application.Jobs;

namespace SpotiHub.Core.Application.Services.Scheduler;

public class SchedulerService : ISchedulerService
{
    private readonly ISchedulerFactory _schedulerFactory;

    public SchedulerService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task Schedule(string user, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var key = JobKey.Create($"job{user}");
        
        var previousJob = await scheduler.GetJobDetail(key, cancellationToken);

        if (previousJob is not null)
        {
            await scheduler.DeleteJob(key, cancellationToken);
        }
        
        var job = JobBuilder.Create<UpdateStatusJob>()
            .WithIdentity($"job{user}")
            .UsingJobData("user", user)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger:{user}")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(30)
                .RepeatForever())
            .Build();
    
        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}