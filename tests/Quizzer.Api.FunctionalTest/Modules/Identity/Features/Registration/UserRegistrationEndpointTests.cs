using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Constants;
using Modules.Identity.Features.Registration;
using Newtonsoft.Json;
using Quizzer.Api.FunctionalTest.Abstraction;
using Shared.Core;

namespace Quizzer.Api.FunctionalTest.Modules.Identity.Features.Registration;
public class UserRegistrationEndpointTests(QuizzerWebApiFactory factory) : QuizzerBaseFunctionTest(factory)
{
    [Fact]
    public async Task Should_CreateUserSuccessfully_WhenRegisterRequestIsValid()
    {
        //Arrange

        var registerUserCommand = GenerateUserRegistrationCommand();

        //Act

        HttpResponseMessage response =
            await HttpClient.PostAsJsonAsync(IdentityModuleConstants.Route.Register, registerUserCommand);

        //Assert

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Should_ReturnValidationError_WhenFullNameIsNotSupplied()
    {
        //Arrange

        var registerUserCommand = GenerateUserRegistrationCommand();

        registerUserCommand = registerUserCommand with
        {
            FullName = ""
        };

        //Act

        HttpResponseMessage response =
            await HttpClient.PostAsJsonAsync(IdentityModuleConstants.Route.Register, registerUserCommand);

        //Assert

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        responseContent.Should().NotBeNull();
        var errors = JsonConvert.DeserializeObject<List<Error>>(responseContent?.Extensions["errors"]?.ToString() ?? string.Empty);
        errors?.FirstOrDefault()?.Message.Should().Be("Full name can not be empty");
    }

    [Fact]
    public async Task Should_AddCreatedDate_WhenUserRegisteredSuccessfully()
    {
        //Arrange

        var registerUserCommand = GenerateUserRegistrationCommand();

        //Act

        HttpResponseMessage response =
            await HttpClient.PostAsJsonAsync(IdentityModuleConstants.Route.Register, registerUserCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userDetails = await UserManager.FindByEmailAsync(registerUserCommand.Email);
        userDetails.Should().NotBeNull();

        userDetails?.CreatedDate.Date.Should().Be(DateTime.UtcNow.Date);
    }

    #region Private methods

    private static UserRegistrationCommand GenerateUserRegistrationCommand()
    {
        var command = new Faker<UserRegistrationCommand>()
            .CustomInstantiator(f => new UserRegistrationCommand(
                f.Name.FullName(),
                f.Internet.Email(),
                "123456@Qa"))
            .Generate();
        return command;
    }


    #endregion
}
