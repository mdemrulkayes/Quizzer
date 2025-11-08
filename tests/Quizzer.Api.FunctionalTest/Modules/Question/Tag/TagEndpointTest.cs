using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Application.Tag.Create;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Application.Tag.Update;
using Modules.Quiz.Core;
using Quizzer.Api.FunctionalTest.Abstraction;
using Quizzer.Api.FunctionalTest.DataCollection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Quizzer.Api.FunctionalTest.Modules.Question.Tag;

public sealed class TagEndpointTest : QuizzerBaseFunctionTest, IDisposable
{
    public TagEndpointTest(QuizzerWebApiFactory factory) : base(factory)
    {
        RegisterOneTimeUser().Wait();
        LoginOneTimeUser().Wait();
    }

    [Theory]
    [ClassData(typeof(TagDataCollection))]
    public async Task Should_ReturnCreatedTagResponse_WhenValidCreateTagCommand(string tagName, string tagDescription)
    {
        AddTokenToEachRequest();

        var tagDataResponse = await HttpClient.PostAsJsonAsync(QuestionModuleConstants.Route.TagRoute.CreateTag, new CreateTagCommand(tagName, tagDescription));
        tagDataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        tagDataResponse.Content.Should().NotBeNull();
        var tagData = await tagDataResponse.Content.ReadFromJsonAsync<TagResponse>();
        tagData.Should().NotBeNull();
        tagData!.Name.Should().Be(tagName);

        if (string.IsNullOrEmpty(tagDescription))
        {
            tagData.Description.Should().BeNull();
        }
        else
        {
            tagData.Description.Should().Be(tagDescription);
        }
    }

    [Theory]
    [ClassData(typeof(DuplicateTagDataCollection))]
    public async Task Should_ReturnValidationError_WhenCreatingDuplicateTag(string tagName, string tagDescription)
    {
        AddTokenToEachRequest();
        // First creation should succeed
        var firstResponse = await HttpClient.PostAsJsonAsync(QuestionModuleConstants.Route.TagRoute.CreateTag, new CreateTagCommand(tagName, tagDescription));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        // Second creation should fail
        var secondResponse = await HttpClient.PostAsJsonAsync(QuestionModuleConstants.Route.TagRoute.CreateTag, new CreateTagCommand(tagName, tagDescription));
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Tag name already exists");
    }

    [Theory]
    [InlineData("Science")]
    public async Task Should_ReturnSuccess_WhenUpdateTag(string tagName)
    {
        AddTokenToEachRequest();

        var tagDataResponse = await HttpClient.PostAsJsonAsync(QuestionModuleConstants.Route.TagRoute.CreateTag, new CreateTagCommand(tagName, null));
        tagDataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tagData = await tagDataResponse.Content.ReadFromJsonAsync<TagResponse>();
        tagData.Should().NotBeNull();

        var updateTagResponse = await HttpClient
            .PutAsJsonAsync(QuestionModuleConstants.Route.TagRoute.UpdateTag.Replace("{tagId}", $"{tagData!.TagId}"), new UpdateTagCommand(tagData.TagId, "UpdatedScience", "Updated description"));
        updateTagResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedTagData = await updateTagResponse.Content.ReadFromJsonAsync<TagResponse>();
        updatedTagData.Should().NotBeNull();
        updatedTagData!.Name.Should().Be("UpdatedScience");
        updatedTagData.Description.Should().Be("Updated description");
    }

    [Theory]
    [InlineData("Science")]
    public async Task Should_ReturnSuccess_WhenDeleteTagById(string tagName)
    {
        AddTokenToEachRequest();

        var tagDataResponse = await HttpClient.PostAsJsonAsync(QuestionModuleConstants.Route.TagRoute.CreateTag, new CreateTagCommand(tagName, null));
        tagDataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tagData = await tagDataResponse.Content.ReadFromJsonAsync<TagResponse>();
        tagData.Should().NotBeNull();

        var updateTagResponse = await HttpClient.DeleteAsync(QuestionModuleConstants.Route.TagRoute.DeleteTag.Replace("{tagId}", $"{tagData!.TagId}"));
        updateTagResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeletedTagResponse = await HttpClient.GetAsync(QuestionModuleConstants.Route.TagRoute.GetTagDetailsById.Replace("{tagId}", $"{tagData!.TagId}"));
        getDeletedTagResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private void AddTokenToEachRequest()
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    LoggedInUserDictionary.FirstOrDefault(x => x.Key == UserEmailDataCollection.Test1Email).Value);
    }

    public void Dispose()
    {
        HttpClient.DefaultRequestHeaders.Authorization = null;
        QuestionModuleDbContext.Tags.ExecuteDeleteAsync().Wait();
    }
}
