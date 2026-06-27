using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Repositories;
using TaskApi.Services;
using TaskApi.Validation;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository
// AddScoped = one instance per HTTP request (correct for database contexts)
builder.Services.AddScoped<ITaskRepository, SqliteTaskRepository>();
builder.Services.AddScoped<ITaskService, TaskServices>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = new ErrorResponse(new ErrorBody(
            ErrorCodes.InternalError,
            "An unexpected error occurred."
        ));

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    });
});
 
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }