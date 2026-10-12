using System.Text.Json.Serialization;
using ApprovalService.Endpoints;
using ApprovalService.Extensions;
using ApprovalService.Middlewares;
using ApprovalService.Repositories;
using ApprovalService.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.AddCustomSwagger();
builder.AddCustomScalar();
builder.AddCustomSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
builder.Services.AddScoped<IApprovalRequestService, ApprovalRequestService>();

builder.Services.AddHttpClient<IUserDirectoryClient, UserDirectoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UsersService:BaseUrl"] ?? "http://localhost:5047");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.AddCustomAuthentication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied");
    }
    catch (Exception exception)
    {
        logger.LogCritical(exception, "Database migration failed, application cannot start");
        throw;
    }
}

app.UseCustomSerilog();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // swagger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Approval Service API v1");
        options.RoutePrefix = "swagger";
    });
    // scalar
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Approval Service API Documentation");
        options.WithTheme(ScalarTheme.Default);
    });
}

app.MapApprovalRequestEndpoints();
app.MapGet("/", () => Results.Redirect("/scalar"));

app.Run();
