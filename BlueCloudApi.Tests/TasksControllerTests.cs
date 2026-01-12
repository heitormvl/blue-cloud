using System.Net;
using System.Net.Http.Json;
using BlueCloudApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlueCloudApi.Tests;

public class TasksControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SqliteConnection _connection;

    public TasksControllerTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
                
                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    
                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                }
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetTasks_ReturnsEmptyList_WhenNoTasksExist()
    {
        // Act
        var response = await _client.GetAsync("/Tasks");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskModel { Title = "Test Task", IsCompleted = false };

        // Act
        var response = await _client.PostAsJsonAsync("/Tasks", newTask);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdTask = await response.Content.ReadFromJsonAsync<TaskModel>();
        Assert.NotNull(createdTask);
        Assert.NotEqual(0, createdTask.Id);
        Assert.Equal(newTask.Title, createdTask.Title);
        Assert.Equal(newTask.IsCompleted, createdTask.IsCompleted);
    }

    [Fact]
    public async Task UpdateTask_UpdatesExistingTask()
    {
        // Arrange - Create a task first
        var newTask = new TaskModel { Title = "Task to Update", IsCompleted = false };
        var createResponse = await _client.PostAsJsonAsync("/Tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskModel>();
        
        // Act - Update the task
        createdTask!.IsCompleted = true;
        var updateResponse = await _client.PutAsJsonAsync($"/Tasks/{createdTask.Id}", createdTask);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify update
        var getResponse = await _client.GetAsync($"/Tasks/{createdTask.Id}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskModel>();
        Assert.True(updatedTask!.IsCompleted);
    }

    [Fact]
    public async Task DeleteTask_RemovesTask()
    {
        // Arrange - Create a task first
        var newTask = new TaskModel { Title = "Task to Delete", IsCompleted = false };
        var createResponse = await _client.PostAsJsonAsync("/Tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskModel>();
        
        // Act - Delete the task
        var deleteResponse = await _client.DeleteAsync($"/Tasks/{createdTask!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/Tasks/{createdTask.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
