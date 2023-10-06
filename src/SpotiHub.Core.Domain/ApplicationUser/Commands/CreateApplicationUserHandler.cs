using Incremental.Common.Sourcing.Abstractions.Commands;
using Incremental.Common.Sourcing.Abstractions.Events;
using MassTransit;
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

    public override async Task Consume(ConsumeContext<CreateApplicationUser> context)
    {

        var user = new Entity.ApplicationUser
        {
            Id = context.Message.ApplicationUserId.ToString(),
            UserName = context.Message.Username,
            Email = context.Message.Email,
            Options = new Entity.Options
            {
                Template = await _templateFinder.GetRandomDefaultTemplate(context.CancellationToken)
            }
        }; 
        
        await _userManager.CreateAsync(user);

        await _eventBus.Publish(new ApplicationUserCreated()
        {
            ApplicationUserId = context.Message.ApplicationUserId
        }, context.CancellationToken);
    }
}