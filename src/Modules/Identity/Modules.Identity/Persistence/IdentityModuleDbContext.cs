using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Constants;
using Modules.Identity.Entities;

namespace Modules.Identity.Persistence;
public class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options) : 
    IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="builder">
    /// The builder being used to construct the model for this context.
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(IdentityModuleConstants.SchemaName);
        builder.Ignore<IdentityPasskeyData>();
        builder.Ignore<IdentityUserPasskey<Guid>>();
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityModuleDbContext).Assembly);
    }
}
