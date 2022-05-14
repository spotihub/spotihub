using Incremental.Common.Sourcing.Abstractions.Commands;
using MediatR;
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

    public override async Task<Unit> Handle(UpdateOptions command, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstAsync(user => user.Id == command.ApplicationUserId.ToString(), cancellationToken);

        if (command.Enabled is not null)
        {
            user.Options.Enabled = command.Enabled.Value;
        }

        if (command.LimitedAvailability is not null)
        {
            user.Options.LimitedAvailability = command.LimitedAvailability.Value;
        }

        if (command.GenreEmojis is not null)
        {
            user.Options.GenreEmojis = command.GenreEmojis.Value;
        }

        if (command.ClearAfter is not null)
        {
            user.Options.ClearAfter = command.ClearAfter;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}