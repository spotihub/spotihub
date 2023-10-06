using Incremental.Common.Sourcing.Abstractions.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SpotiHub.Core.Domain.Contract.ApplicationUser.Commands;
using SpotiHub.Persistence.Context;

namespace SpotiHub.Core.Domain.ApplicationUser.Commands;

public class UpdateOptionsHandler : CommandHandler<UpdateOptions>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateOptionsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task Consume(ConsumeContext<UpdateOptions> context)
    {
        
        var user = await _dbContext.Users.FirstAsync(user => user.Id == context.Message.ApplicationUserId.ToString(), context.CancellationToken);

        if (context.Message.Enabled is not null)
        {
            user.Options.Enabled = context.Message.Enabled.Value;
        }

        if (context.Message.LimitedAvailability is not null)
        {
            user.Options.LimitedAvailability = context.Message.LimitedAvailability.Value;
        }

        if (context.Message.GenreEmojis is not null)
        {
            user.Options.GenreEmojis = context.Message.GenreEmojis.Value;
        }

        if (context.Message.ClearAfter is not null)
        {
            user.Options.ClearAfter = context.Message.ClearAfter;
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}