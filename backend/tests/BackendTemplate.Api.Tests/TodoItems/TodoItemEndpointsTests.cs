using System.Net;
using System.Net.Http.Json;
using BackendTemplate.Api.Tests.Common;
using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.TodoItems;

namespace BackendTemplate.Api.Tests.TodoItems;

public class TodoItemEndpointsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateMigratedClient();

    [Fact]
    public async Task Post_GivenValidRequest_ThenReturns201WithLocation()
    {
        var response = await _client.PostAsJsonAsync("/todo-items", new { title = "Buy milk" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Post_GivenEmptyTitle_ThenReturns422()
    {
        var response = await _client.PostAsJsonAsync("/todo-items", new { title = "" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Get_GivenExistingId_ThenReturns200WithItem()
    {
        var created = await _client.PostAsJsonAsync("/todo-items", new { title = "Fetch me" });
        var location = created.Headers.Location!;

        var response = await _client.GetAsync(location);
        var body = await response.Content.ReadFromJsonAsync<TodoItemResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Fetch me", body!.Title);
    }

    [Fact]
    public async Task Get_GivenNonExistentId_ThenReturns404()
    {
        var response = await _client.GetAsync($"/todo-items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ThenReturnsPagedEnvelope()
    {
        await _client.PostAsJsonAsync("/todo-items", new { title = "Item 1" });
        await _client.PostAsJsonAsync("/todo-items", new { title = "Item 2" });

        var response = await _client.GetAsync("/todo-items?page=1&pageSize=10");
        var body = await response.Content.ReadFromJsonAsync<Page<TodoItemResult>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.Total >= 2);
    }

    [Fact]
    public async Task GetAll_GivenStatusFilter_ThenFiltersResults()
    {
        await _client.PostAsJsonAsync("/todo-items", new { title = "Pending item" });

        var response = await _client.GetAsync("/todo-items?status=Pending");
        var body = await response.Content.ReadFromJsonAsync<Page<TodoItemResult>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.All(body!.Items, i => Assert.Equal(TodoStatus.Pending, i.Status));
    }

    [Fact]
    public async Task Patch_GivenExistingItem_ThenReturns200WithUpdatedItem()
    {
        var created = await _client.PostAsJsonAsync("/todo-items", new { title = "Original" });
        var location = created.Headers.Location!;

        var response = await _client.PatchAsJsonAsync(location,
            new { title = "Updated", status = "InProgress" });
        var body = await response.Content.ReadFromJsonAsync<TodoItemResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Updated", body!.Title);
        Assert.Equal(TodoStatus.InProgress, body.Status);
    }

    [Fact]
    public async Task Delete_GivenExistingItem_ThenReturns204()
    {
        var created = await _client.PostAsJsonAsync("/todo-items", new { title = "Delete me" });
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_GivenNonExistentId_ThenReturns404()
    {
        var response = await _client.DeleteAsync($"/todo-items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
