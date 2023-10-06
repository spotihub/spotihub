using Incremental.Common.Sourcing.Abstractions.Commands;
using Incremental.Common.Sourcing.Abstractions.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SpotiHub.Core.Domain.Contract.ApplicationUser.Commands;
using SpotiHub.Core.Domain.Contract.ApplicationUser.Events;
using SpotiHub.Core.Domain.Contract.Template.Services;

namespace SpotiHub.Core.Domain.ApplicationUser.Commands;

public class CreateApplicationUserHandler : CommandHandler<CreateApplicationUser>
{
    private readonly IEventBus _eventBus;
    private readonly UserManager<Entity.ApplicationUser> _userManager;
    private readonly ITemplateFinder _templateFinder;
    
    public CreateApplicationUserHandler(IEventBus eventBus, UserManager<Entity.ApplicationUser> userManager, ITemplateFinder templateFinder)
    {
        _eventBus = eventBus;
        _userManager = userManager;
        _templateFinder = templateFinder;
    }

    public override async Task<Unit> Handle(CreateApplicationUser command, CancellationToken cancellationToken)
    {
        var user = new Entity.ApplicationUser
        {
            Id = command.ApplicationUserId.ToString(),
            UserName = command.Username,
            Email = command.Email,
            Options = new Entity.Options
            {
                Template = await _templateFinder.GetRandomDefaultTemplate(cancellationToken)
            }
        }; 
        
        await _userManager.CreateAsync(user);

        await _eventBus.Publish(new ApplicationUserCreated()
        {
            ApplicationUserId = command.ApplicationUserId
        }, cancellationToken);
        
        return Unit.Value;
    }
}