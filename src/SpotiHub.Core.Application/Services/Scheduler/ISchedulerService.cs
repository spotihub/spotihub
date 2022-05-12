using System.Threading;
using System.Threading.Tasks;

namespace SpotiHub.Core.Application.Services.Scheduler;

public interface ISchedulerService
{
    Task Schedule(string user, CancellationToken cancellationToken = default);
}