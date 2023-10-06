using Incremental.Common.Authentication;
using Incremental.Common.Sourcing.Abstractions.Commands;
using Microsoft.AspNetCore.Mvc;
using SpotiHub.Core.Domain.Contract.ApplicationUser.Commands;

namespace SpotiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly ICommandBus _commandBus;

    public ConfigurationController(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    [HttpPut(Name = nameof(UpdateOptions))]
    public IActionResult UpdateOptions(UpdateOptionsViewModel model)
    {
        Response.OnCompleted(async () =>
        {
            await _commandBus.Send(new UpdateOptions
            {
                ApplicationUserId = new Guid(User.GetId()),
                Enabled = model.Enabled,
                LimitedAvailability = model.LimitedAvailability,
                GenreEmojis = model.GenreEmojis,
                ClearAfter = model.ClearAfter
            });
        });
        
        return Accepted();
    }
}

public class UpdateOptionsViewModel
{
    public bool? Enabled { get; set; }
    public bool? LimitedAvailability { get; set; }
    public bool? GenreEmojis { get; set; }
    public TimeSpan? ClearAfter { get; set; }
}