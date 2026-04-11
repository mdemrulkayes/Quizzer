using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Constants;
using Modules.Identity.Entities;
using Modules.Identity.Features.Login;
using Modules.Identity.Features.Registration;
using Modules.Identity.Persistence;
using Modules.Quiz.Infrastructure.Data;
using Quizzer.Api.FunctionalTest.DataCollection;
using Shared.Core;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Abstraction;
public class QuizzerBaseFunctionTest
    : IClassFixture<QuizzerWebApiFactory>, IDisposable
{
    private readonly QuizzerWebApiFactory _factory;
    private readonly IServiceScope _scope;
    protected readonly HttpClient HttpClient;
    protected readonly UserManager<ApplicationUser> UserManager;
    public Dictionary<string, string> LoggedInUserDictionary = new();
    protected readonly ITimeProvider TimeProvider;
    protected readonly QuestionModuleDbContext QuestionModuleDbContext;

    public QuizzerBaseFunctionTest(QuizzerWebApiFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        HttpClient = factory.CreateClient();
        UserManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        TimeProvider = _scope.ServiceProvider.GetRequiredService<ITimeProvider>();
        QuestionModuleDbContext = _scope.ServiceProvider.GetRequiredService<QuestionModuleDbContext>();
    }

    public async Task RegisterOneTimeUser()
    {
        foreach (var registrationCommand in GenerateRegisterUserCommand())
        {
            await HttpClient.PostAsJsonAsync(IdentityModuleConstants.Route.Register, registrationCommand);
        }

        // Promote test2 to QuizAuthor role using raw SQL to avoid EF tracking/PK issues
        using var roleScope = _factory.Services.CreateScope();
        var identityDb = roleScope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();
        await identityDb.Database.ExecuteSqlRawAsync(@"
            DELETE ur FROM [Identity].[UserRoles] ur
            INNER JOIN [Identity].[Users] u ON ur.UserId = u.Id
            INNER JOIN [Identity].[Roles] r ON ur.RoleId = r.Id
            WHERE u.NormalizedEmail = 'TEST2@GMAIL.COM' AND r.NormalizedName = 'EXAMINE';

            INSERT INTO [Identity].[UserRoles] (UserId, RoleId)
            SELECT u.Id, r.Id
            FROM [Identity].[Users] u
            CROSS JOIN [Identity].[Roles] r
            WHERE u.NormalizedEmail = 'TEST2@GMAIL.COM'
            AND r.NormalizedName = 'QUIZAUTHOR'
            AND NOT EXISTS (
                SELECT 1 FROM [Identity].[UserRoles] ur2
                WHERE ur2.UserId = u.Id AND ur2.RoleId = r.Id
            );
        ");
    }

    public async Task LoginOneTimeUser()
    {
        foreach (var loginCommand in GenerateRegisterUserCommand())
        {
            var loginApiCall = await HttpClient.PostAsJsonAsync(IdentityModuleConstants.Route.Login, loginCommand);
            var content = await loginApiCall.Content.ReadFromJsonAsync<AccessTokenResponse>();

            LoggedInUserDictionary.Add(loginCommand.Email, content!.Token);
        }
    }

    private static List<UserRegistrationCommand> GenerateRegisterUserCommand()
    {
        var faker = new Faker();
        return
        [
            new UserRegistrationCommand(faker.Name.FullName(), "test1@gmail.com", "Aa123456#"),
            new UserRegistrationCommand(faker.Name.FullName(), "test2@gmail.com", "Aa123456!"),
            new UserRegistrationCommand(faker.Name.FullName(), "test3@gmail.com", "Aa123456%")
        ];
    }

    public void Dispose()
    {
        LoggedInUserDictionary.Clear();
        LoggedInUserDictionary = new();
        _scope.Dispose();
        HttpClient.Dispose();
        UserManager.Dispose();
    }

    /// <summary>
    /// Sets the default Authorization header to use the QuizAuthor (test2) token.
    /// Use this for tests that need QuizAuthor-level access (tags, question sets, etc.)
    /// </summary>
    internal void AddTokenToEachRequest()
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    LoggedInUserDictionary.FirstOrDefault(x => x.Key == UserEmailDataCollection.Test2Email).Value);
    }
}
