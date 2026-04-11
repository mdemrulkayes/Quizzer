using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.Identity.Constants;
using Modules.Identity.Entities;
using Modules.Identity.Models;
using Serilog;
using Shared.Core;

namespace Modules.Identity;

public static class SeedIdentityData
{
    /// <summary>
    /// Not required not as all the role are inserted through migration
    /// </summary>
    /// <param name="application"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    internal static IApplicationBuilder SeedDefaultRoles(this IApplicationBuilder application, ILogger logger)
    {
        logger.Information("Inside seed default roles.");
        var scopedService = application.ApplicationServices.CreateScope();
        
        var roleManager = scopedService.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!roleManager.Roles.Any(x => x.Name == RoleConstants.SuperAdmin))
        {
            logger.Information("Adding super admin roles.");
            roleManager.CreateAsync(new IdentityRole<Guid>(RoleConstants.SuperAdmin)).Wait();
        }
        
        if (!roleManager.Roles.Any(x => x.Name == RoleConstants.SupportAdmin))
        {
            logger.Information("Adding support admin roles.");
            roleManager.CreateAsync(new IdentityRole<Guid>(RoleConstants.SupportAdmin)).Wait();
        }
        
        if (!roleManager.Roles.Any(x => x.Name == RoleConstants.Examine))
        {
            logger.Information("Adding Examine roles.");
            roleManager.CreateAsync(new IdentityRole<Guid>(RoleConstants.Examine)).Wait();
        }
        
        if (!roleManager.Roles.Any(x => x.Name == RoleConstants.QuizAuthor))
        {
            logger.Information("Adding QuizAuthor roles.");
            roleManager.CreateAsync(new IdentityRole<Guid>(RoleConstants.QuizAuthor)).Wait();
        }
        
        return application;
    }

    /// <summary>
    /// Add default super admin from appsettings.json
    /// </summary>
    /// <param name="application"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    internal static IApplicationBuilder SeedDefaultUsers(this IApplicationBuilder application, ILogger logger)
    {
        logger.Information("Inside seed default users.");
        
        var scopedService = application.ApplicationServices.CreateScope();
        
        var userManager = scopedService.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = scopedService.ServiceProvider.GetRequiredService<IOptions<DefaultUser>>().Value;
        var timeProvider = scopedService.ServiceProvider.GetRequiredService<ITimeProvider>();
        
        try
        {
            var applicationUser = ApplicationUser
                .RegisterInternalAdminUser(user.FirstName, user.LastName, user.Email, user.PhoneNumber,
                    timeProvider).Value;
            
            var isUserExist = userManager.Users.Any(x => x.Email == user.Email);
            
            if (applicationUser is not null && !isUserExist)
            {
                userManager.CreateAsync(applicationUser, user.Password).Wait();
                logger.Information("Default admin user created successfully.");

                userManager.AddToRoleAsync(applicationUser, RoleConstants.SuperAdmin).Wait();
                logger.Information("Default admin user added to the role successfully.");
            }
        }
        catch (Exception e)
        {
            logger.Error(exception: e, $"Failed to register user {user.FirstName} {user.LastName}.");
        }
        return application;
    }
}