using FluentAssertions;
using Modules.AI.Application.Dtos;
using Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;
using Modules.AI.Core;
using Quizzer.Api.FunctionalTest.Abstraction;
using System.Net;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Modules.AI;

public class AIProviderConfigEndpointTest : QuizzerBaseFunctionTest
{
    public AIProviderConfigEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }

    [Fact]
    public async Task GetSupportedProviders_ShouldReturnProviders()
    {
        // Arrange
        AddTokenToEachRequest();

        // Act
        var response = await HttpClient.GetAsync(AIModuleConstants.Route.ProviderConfig.GetSupportedProviders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var providers = await response.Content.ReadFromJsonAsync<List<SupportedProviderDto>>();
        providers.Should().NotBeNull();
        providers.Should().HaveCount(2);
        providers!.Select(p => p.ProviderId).Should().Contain("gemini");
        providers.Select(p => p.ProviderId).Should().Contain("groq");
    }

    [Fact]
    public async Task SaveProviderConfig_WithValidData_ShouldReturnOk()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("gemini", "test-api-key-that-is-long-enough");

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<ProviderConfigResponse>();
        config.Should().NotBeNull();
        config!.ProviderId.Should().Be("gemini");
        config.ProviderName.Should().Be("Google Gemini");
        config.IsActive.Should().BeTrue();
        config.MaskedApiKey.Should().Be("****...configured");
    }

    [Fact]
    public async Task GetProviderConfig_AfterSave_ShouldReturnConfig()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("gemini", "test-api-key-that-is-long-enough");
        await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Act
        var response = await HttpClient.GetAsync(AIModuleConstants.Route.ProviderConfig.GetProviderConfig);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<ProviderConfigResponse>();
        config.Should().NotBeNull();
        config!.ProviderId.Should().Be("gemini");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetProviderConfig_WhenNotConfigured_ShouldReturnNotFound()
    {
        // Arrange — use test1 (Examinee) who has no config saved
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                LoggedInUserDictionary.FirstOrDefault(x => x.Key == "test1@gmail.com").Value);

        // Delete any existing config first to ensure clean state
        await HttpClient.DeleteAsync(AIModuleConstants.Route.ProviderConfig.DeleteProviderConfig);

        // Act
        var response = await HttpClient.GetAsync(AIModuleConstants.Route.ProviderConfig.GetProviderConfig);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SaveProviderConfig_WithInvalidProvider_ShouldReturnBadRequest()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("invalid-provider", "test-api-key-that-is-long-enough");

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveProviderConfig_WithEmptyKey_ShouldReturnBadRequest()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("gemini", "");

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveProviderConfig_WithShortKey_ShouldReturnBadRequest()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("gemini", "short");

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProviderConfig_AfterSave_ShouldReturnOk()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("gemini", "test-api-key-that-is-long-enough");
        await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Act
        var response = await HttpClient.DeleteAsync(AIModuleConstants.Route.ProviderConfig.DeleteProviderConfig);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteProviderConfig_WhenNotConfigured_ShouldReturnNotFound()
    {
        // Arrange — use test3 (Examinee) who has no config saved
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                LoggedInUserDictionary.FirstOrDefault(x => x.Key == "test3@gmail.com").Value);

        // Ensure clean state
        await HttpClient.DeleteAsync(AIModuleConstants.Route.ProviderConfig.DeleteProviderConfig);

        // Act — second delete should fail
        var response = await HttpClient.DeleteAsync(AIModuleConstants.Route.ProviderConfig.DeleteProviderConfig);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestProviderConnection_AfterSave_ShouldReturnSuccess()
    {
        // Arrange
        AddTokenToEachRequest();
        var command = new SaveProviderConfigCommand("gemini", "test-api-key-that-is-long-enough");
        await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, command);

        // Act
        var response = await HttpClient.PostAsync(
            AIModuleConstants.Route.ProviderConfig.TestProviderConnection, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var testResult = await response.Content.ReadFromJsonAsync<TestConnectionResponse>();
        testResult.Should().NotBeNull();
        testResult!.Success.Should().BeTrue();
        testResult.Message.Should().Be("Connection successful.");
    }

    [Fact]
    public async Task SaveProviderConfig_CanUpdateExistingConfig()
    {
        // Arrange
        AddTokenToEachRequest();
        var firstCommand = new SaveProviderConfigCommand("gemini", "first-api-key-long-enough");
        await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, firstCommand);

        var updateCommand = new SaveProviderConfigCommand("groq", "updated-api-key-long-enough");

        // Act
        var response = await HttpClient.PostAsJsonAsync(
            AIModuleConstants.Route.ProviderConfig.SaveProviderConfig, updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<ProviderConfigResponse>();
        config.Should().NotBeNull();
        config!.ProviderId.Should().Be("groq");
        config.ProviderName.Should().Be("Groq (Llama 3)");
    }

    [Fact]
    public async Task Endpoints_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange — no token set
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var getProvidersResponse = await HttpClient.GetAsync(
            AIModuleConstants.Route.ProviderConfig.GetSupportedProviders);
        var getConfigResponse = await HttpClient.GetAsync(
            AIModuleConstants.Route.ProviderConfig.GetProviderConfig);

        // Assert
        getProvidersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        getConfigResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
