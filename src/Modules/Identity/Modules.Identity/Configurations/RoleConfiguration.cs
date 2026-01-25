using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Constants;

namespace Modules.Identity.Configurations;
internal sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder
            .ToTable("Roles")
            .HasKey(x => x.Id);

        builder.HasData(new List<IdentityRole<Guid>> {
            new()
            {
                Id = Guid.Parse(IdentityModuleConstants.Role.SuperAdminRoleGuid),
                Name = RoleConstants.SuperAdmin,
                NormalizedName = RoleConstants.SuperAdmin,
                ConcurrencyStamp = "STATIC-ADMIN-STAMP"
            },
            new()
            {
                Id = Guid.Parse(IdentityModuleConstants.Role.SupportAdminRoleGuid),
                Name = RoleConstants.SupportAdmin,
                NormalizedName = RoleConstants.SupportAdmin,
                ConcurrencyStamp = "STATIC-ADMIN-STAMP"
            },
            new()
            {
                Id = Guid.Parse(IdentityModuleConstants.Role.QuizAuthorRoleGuid),
                Name = RoleConstants.QuizAuthor,
                NormalizedName = RoleConstants.QuizAuthor,
                ConcurrencyStamp = "STATIC-ADMIN-STAMP"
            },
            new()
            {
                Id = Guid.Parse(IdentityModuleConstants.Role.ExamineRoleGuid),
                Name = RoleConstants.Examine,
                NormalizedName = RoleConstants.Examine,
                ConcurrencyStamp = "STATIC-ADMIN-STAMP"
            }
        });
    }
}