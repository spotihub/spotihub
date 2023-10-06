using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotiHub.Core.Entity;

namespace SpotiHub.Persistence.Configuration;

public class ApplicationUserEntityTypeConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.OwnsOne(e => e.Options, builder =>
        {
            builder.ToTable("Options");
        });
    }
}