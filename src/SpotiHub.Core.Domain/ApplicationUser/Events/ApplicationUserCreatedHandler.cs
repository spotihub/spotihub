using Microsoft.EntityFrameworkCore;
using SpotiHub.Core.Domain.Contract.ApplicationUser.Events;
using SpotiHub.Core.Domain.Contract.Template.Services;
using SpotiHub.Persistence.Context;

namespace SpotiHub.Core.Domain.ApplicationUser.Events;

public class ApplicationUserCreatedHandler : Incremental.Common.Sourcing.Abstractions.Events.EventHandler<ApplicationUserCreated>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITemplateFinder _templateFinder;

    public ApplicationUserCreatedHandler(ApplicationDbContext dbContext, ITemplateFinder templateFinder)
    {
        _dbContext = dbContext;
        _templateFinder = templateFinder;
    }

    public override async Task Handle(ApplicationUserCreated @event, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstAsync(user => user.Id == @event.ApplicationUserId.ToString(), cancellationToken);

        user.Options.Template = await _templateFinder.GetRandomDefaultTemplate(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}