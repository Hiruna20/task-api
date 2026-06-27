using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskApi.Tests;

public class TasksApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // --- CREATE ---

    [Fact]
    public async Task Create_ValidTask_Returns201WithDefaults()
    {
        var payload = new { title = "Write API tests", priority = "high" };

        var response = await _client.PostAsJsonAsync("/api/v1/tasks", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var root = body!.RootElement;

        Assert.False(string.IsNullOrEmpty(root.GetProperty("id").GetString()));
        Assert.Equal("Write API tests", root.GetProperty("title").GetString());
        Assert.Equal("todo", root.GetProperty("status").GetString());     
        Assert.Equal("high", root.GetProperty("priority").GetString());
    }

    [Fact]
    public async Task Create_MissingTitle_Returns400()
    {
        var payload = new { priority = "high" };

        var response = await _client.PostAsJsonAsync("/api/v1/tasks", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var error = body!.RootElement.GetProperty("error");
        Assert.Equal("VALIDATION_ERROR", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Create_DefaultsPriorityToMedium_WhenNotProvided()
    {
        var payload = new { title = "No priority set" };

        var response = await _client.PostAsJsonAsync("/api/v1/tasks", payload);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.Equal("medium", body!.RootElement.GetProperty("priority").GetString());
    }

    // --- LIST ---

    [Fact]
    public async Task List_ReturnsTasksWithPagination()
    {
        await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "List test task" });

        var response = await _client.GetAsync("/api/v1/tasks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var root = body!.RootElement;

        Assert.True(root.GetProperty("data").GetArrayLength() > 0);
        Assert.True(root.GetProperty("pagination").TryGetProperty("total", out _));
    }

    [Fact]
    public async Task List_FilterByStatus_ReturnsOnlyMatchingTasks()
    {
        await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "Filter by status task" });

        var response = await _client.GetAsync("/api/v1/tasks?status=todo");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        foreach (var item in body!.RootElement.GetProperty("data").EnumerateArray())
        {
            Assert.Equal("todo", item.GetProperty("status").GetString());
        }
    }

    [Fact]
    public async Task List_InvalidStatusFilter_Returns400()
    {
        var response = await _client.GetAsync("/api/v1/tasks?status=bogus");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- GET BY ID ---

    [Fact]
    public async Task GetById_ExistingTask_Returns200()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "Get by id task" });
        var created = await create.Content.ReadFromJsonAsync<JsonDocument>();
        var id = created!.RootElement.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/v1/tasks/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_MissingTask_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/tasks/does_not_exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var error = body!.RootElement.GetProperty("error");
        Assert.Equal("TASK_NOT_FOUND", error.GetProperty("code").GetString());
    }

    // --- UPDATE ---

    [Fact]
    public async Task Update_ValidStatusChange_Returns200()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "Update task" });
        var created = await create.Content.ReadFromJsonAsync<JsonDocument>();
        var id = created!.RootElement.GetProperty("id").GetString();

        var response = await _client.PatchAsync($"/api/v1/tasks/{id}",
            JsonContent.Create(new { status = "in_progress" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("in_progress", body!.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Update_InvalidStatus_Returns400()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "Bad status update" });
        var created = await create.Content.ReadFromJsonAsync<JsonDocument>();
        var id = created!.RootElement.GetProperty("id").GetString();

        var response = await _client.PatchAsync($"/api/v1/tasks/{id}",
            JsonContent.Create(new { status = "bogus" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_EmptyBody_Returns400()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "Empty update body task" });
        var created = await create.Content.ReadFromJsonAsync<JsonDocument>();
        var id = created!.RootElement.GetProperty("id").GetString();

        var response = await _client.PatchAsync($"/api/v1/tasks/{id}",
            JsonContent.Create(new { }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_MissingTask_Returns404()
    {
        var response = await _client.PatchAsync("/api/v1/tasks/does_not_exist",
            JsonContent.Create(new { status = "done" }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- DELETE ---

    [Fact]
    public async Task Delete_ExistingTask_Returns204()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/tasks", new { title = "Delete task" });
        var created = await create.Content.ReadFromJsonAsync<JsonDocument>();
        var id = created!.RootElement.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/api/v1/tasks/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // confirm it's actually gone
        var getResponse = await _client.GetAsync($"/api/v1/tasks/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_MissingTask_Returns404()
    {
        var response = await _client.DeleteAsync("/api/v1/tasks/does_not_exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}