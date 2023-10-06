using Microsoft.AspNetCore.Identity;

namespace SpotiHub.Core.Entity;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
        Templates = new List<Template>();
    }
    public Options Options { get; set; } = null!;
    public List<Template> Templates { get; set; }
}