using CommunityHelper.API.Middleware;
using CommunityHelper.API.Realtime;
using CommunityHelper.Application;
using CommunityHelper.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string WebAppCorsPolicy = "WebAppCorsPolicy";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy(WebAppCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Must run first so it can catch exceptions from everything downstream.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(WebAppCorsPolicy);

// Browsers do NOT apply CORS to WebSocket handshakes, so the origin allow-list
// here is the only thing preventing cross-site WebSocket hijacking. Requests
// with no Origin header (native clients, tests) are unaffected.
var webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) };
foreach (var origin in allowedOrigins)
{
    webSocketOptions.AllowedOrigins.Add(origin);
}

app.UseWebSockets(webSocketOptions);
app.UseAuthorization();

app.MapControllers();
app.MapRealtimeEndpoint();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposed so integration tests can reference this entry point via WebApplicationFactory.
public partial class Program;
