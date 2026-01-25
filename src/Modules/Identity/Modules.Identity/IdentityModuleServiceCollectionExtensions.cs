using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Persistence;
using Modules.Identity.Constants;
using Modules.Identity.Entities;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Features.Login;
using Modules.Identity.Features.Login.Services;
using Modules.Identity.Features.Registration.Services;
using Modules.Identity.Models;
using Shared.Infrastructure.Interceptors;

namespace Modules.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterIdentityModule(this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        List<Assembly> mediatRAssembly)
    {
        services
            .RegisterIdentityDatabase(logger, configuration);

        mediatRAssembly.Add(typeof(ServiceCollectionExtensions).Assembly);


        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        services.AddScoped<ILoginService, LoginService>();
        services.Configure<JwtConfiguration>(configuration.GetSection(nameof(JwtConfiguration)));
        services.Configure<DefaultUser>(configuration.GetSection(nameof(DefaultUser)));

        logger.Information("{Module} registered successfully", "Identity");

        return services;
    }

    private static void RegisterIdentityDatabase(this IServiceCollection services, ILogger logger,
        IConfiguration configuration)
    {
        services.AddScoped<PopulateAuditableEntityInterceptor>();
        services.AddDbContext<IdentityModuleDbContext>((sp, opt) =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("IdentityModuleDbContext"), optBuilder =>
            {
                optBuilder.EnableRetryOnFailure(10);
                optBuilder.MigrationsHistoryTable(IdentityModuleConstants.MigrationHistoryTableName,
                    IdentityModuleConstants.SchemaName);
                optBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }).AddInterceptors(sp.GetRequiredService<PopulateAuditableEntityInterceptor>());
        });

        RegisterIdentity(services, configuration);
        logger.Information("Identity module db context registered");
    }

    private static void RegisterIdentity(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
            options.User.RequireUniqueEmail = false;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        });

        var jwtConfig = configuration
            .GetSection(nameof(JwtConfiguration))
            .Get<JwtConfiguration>();

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.Audience = jwtConfig?.JwtAudience;
            opt.SaveToken = true;
            opt.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = jwtConfig?.JwtAudience,
                ValidIssuer = jwtConfig?.JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(jwtConfig!.JwtKey)),
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };
        });

        services.AddAuthorization();

        services.AddIdentityCore<ApplicationUser>(
            opt =>
            {
                opt.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddEntityFrameworkStores<IdentityModuleDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));
    }

    public static IApplicationBuilder MigrateIdentityModuleDatabase(this IApplicationBuilder app, ILogger logger)
    {
        var scopedService = app.ApplicationServices.CreateScope();
        var dbContext = scopedService.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();

        if (dbContext.Database.IsSqlServer())
        {
            dbContext.Database.Migrate();
        }

        app.SeedDefaultRoles(logger)
            .SeedDefaultUsers(logger);

        return app;
    }
}
