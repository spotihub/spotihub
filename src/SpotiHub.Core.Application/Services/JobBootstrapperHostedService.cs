using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotiHub.Core.Application.Services.Scheduler;
using SpotiHub.Persistence.Context;

namespace SpotiHub.Core.Application.Services;

public class JobBootstrapperHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JobBootstrapperHostedService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var scheduler = scope.ServiceProvider.GetRequiredService<SchedulerService>();

        var users = await context.UserClaims.AsNoTracking()
            .Where(claim => claim.ClaimType == "spotify:refresh_token")
            .Select(claim => claim.UserId)
            .ToListAsync(cancellationToken: stoppingToken);
        
        foreach (var user in users)
        {
            await scheduler.Schedule(user, stoppingToken);
        }
    }
}