using Microsoft.AspNetCore.Identity;

namespace SpotiHub.Core.Entity;

public class ApplicationUser : IdentityUser
{
    public Options Options { get; set; }
}