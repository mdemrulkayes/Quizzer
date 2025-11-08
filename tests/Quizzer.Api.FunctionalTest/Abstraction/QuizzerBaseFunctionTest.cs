using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Constants;
using Modules.Identity.Entities;
using Modules.Identity.Features.Login;
using Modules.Identity.Features.Registration;
using Modules.Identity.Features.Registration.Enums;
using Modules.Quiz.Infrastructure.Data;
using Quizzer.Api.FunctionalTest.DataCollection;
using Shared.Core;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Abstraction;
public class QuizzerBaseFunctionTest
    : IClassFixture<QuizzerWebApiFactory>, IDisposable
{
    private readonly IServiceScope _scope;
    protected readonly HttpClient HttpClient;
    protected readonly UserManager<ApplicationUser> UserManager;
    public Dictionary<string, string> LoggedInUserDictionary = new();
    protected readonly ITimeProvider TimeProvider;
    protected readonly QuestionModuleDbContext QuestionModuleDbContext;

    public QuizzerBaseFunctionTest(QuizzerWebApiFactory factory)
    {
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
            new UserRegistrationCommand(faker.Name.FirstName(), faker.Name.LastName(), "test1@gmail.com", faker.Phone.PhoneNumber(), "Aa123456#", "Aa123456#",
                UserType.Examine),

            new UserRegistrationCommand(faker.Name.FirstName(), faker.Name.LastName(), "test2@gmail.com", faker.Phone.PhoneNumber(), "Aa123456!", "Aa123456!",
                UserType.QuizAuthor),

            new UserRegistrationCommand(faker.Name.FirstName(), faker.Name.LastName(), "test3@gmail.com", faker.Phone.PhoneNumber(), "Aa123456%", "Aa123456%",
                UserType.QuizAuthor)
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

    internal void AddTokenToEachRequest()
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    LoggedInUserDictionary.FirstOrDefault(x => x.Key == UserEmailDataCollection.Test1Email).Value);
    }
}
