using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using CommunityHelper.API.Middleware;
using CommunityHelper.API.Realtime;
using CommunityHelper.Application;
using CommunityHelper.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

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

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Secret must be at least 32 characters. Set it via configuration or the Jwt__Secret environment variable.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidateAudience = true,
            ValidAudience = jwtSection.GetValue<string>("Audience"),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRealtimeEndpoint();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposed so integration tests can reference this entry point via WebApplicationFactory.
public partial class Program;
