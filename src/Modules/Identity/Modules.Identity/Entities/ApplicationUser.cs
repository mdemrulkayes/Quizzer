using Microsoft.AspNetCore.Identity;
using Modules.Identity.Features.Registration;
using Modules.Identity.Features.Registration.Enums;
using Shared.Core;

namespace Modules.Identity.Entities;
public sealed class ApplicationUser : IdentityUser<Guid>, IUpdatedAuditableEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserType UserType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedDate { get; private set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }

    public DateTimeOffset? LastLoginTime { get; set; }

    private ApplicationUser()
    {
        
    }
    private ApplicationUser(string firstName, string lastName, string email, string? phoneNumber, UserType userType, ITimeProvider timeProvider)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = UserName = NormalizedEmail = NormalizedUserName = email;
        PhoneNumber = phoneNumber;
        EmailConfirmed = true;
        PhoneNumberConfirmed = phoneNumber is not null;
        CreatedDate = timeProvider.TimeNow;
        UserType = userType;
    }

    internal static Result<ApplicationUser> RegisterUser(string firstName, string lastName, string email, string? phoneNumber, UserType userType, ITimeProvider timeProvider)
    {
        if (userType is UserType.SuperAdmin or UserType.SupportAdmin)
        {
            return RegistrationErrors.InvalidUserTypeToRegistrationFlow;
        }

        return new ApplicationUser(firstName, lastName, email, phoneNumber, userType, timeProvider);
    }

    internal static Result<ApplicationUser> RegisterInternalAdminUser(string firstName, string lastName, string email,
        string phoneNumber, ITimeProvider timeProvider)
    {
        return new ApplicationUser(firstName, lastName, email, phoneNumber, UserType.SuperAdmin, timeProvider);
    }

    internal void UpdateLastLoginTime(ITimeProvider timeProvider)
    {
        LastLoginTime = timeProvider.TimeNow;
        UpdatedDate = timeProvider.TimeNow;
    }
}
