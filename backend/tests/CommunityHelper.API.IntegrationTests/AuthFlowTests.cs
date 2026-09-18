using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CommunityHelper.API.IntegrationTests;

/// <summary>
/// End-to-end coverage for ticket 01: register, login, refresh rotation with
/// reuse detection, logout revocation, role gating, and owner-only profile
/// and organization CRUD. Each test uses a unique email because the
/// in-memory stores are shared within this fixture.
/// </summary>
public class AuthFlowTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    private static int _counter;

    private static string UniqueEmail(string prefix) =>
        $"{prefix}-{Interlocked.Increment(ref _counter)}-{Guid.NewGuid():N}@example.com";

    private HttpClient CreateClient() => factory.CreateClient();

    private static void UseAccessToken(HttpClient client, string accessToken) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    private async Task<JsonElement> RegisterAsync(
        HttpClient client, string email, string password, string role, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password, role },
            ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    [Fact]
    public async Task Register_ReturnsTokenPair_And_MeReturnsIdentity()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();
        var email = UniqueEmail("volunteer");

        var body = await RegisterAsync(client, email, "password123", "volunteer", cts.Token);

        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("refreshToken").GetString()));
        Assert.Equal("volunteer", body.GetProperty("user").GetProperty("role").GetString());

        UseAccessToken(client, body.GetProperty("accessToken").GetString()!);
        var me = await client.GetAsync("/api/auth/me", cts.Token);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        var identity = await me.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        Assert.Equal(email.ToLowerInvariant(), identity.GetProperty("email").GetString());
        Assert.Equal("volunteer", identity.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsProblemDetails()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();
        var email = UniqueEmail("dupe");

        await RegisterAsync(client, email, "password123", "volunteer", cts.Token);

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password = "password123", role = "volunteer" },
            cts.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Register_InvalidInput_ReturnsValidationErrors()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "not-an-email", password = "short", role = "corp_admin" },
            cts.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.True(errors.TryGetProperty("Password", out _));
        Assert.True(errors.TryGetProperty("Role", out _));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsUsableTokenPair()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();
        var email = UniqueEmail("login-ok");

        await RegisterAsync(client, email, "password123", "volunteer", cts.Token);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = "password123" },
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(cts.Token);

        UseAccessToken(client, body.GetProperty("accessToken").GetString()!);
        var me = await client.GetAsync("/api/auth/me", cts.Token);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Refresh_EmptyToken_ReturnsValidationErrors()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = "" },
            cts.Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        Assert.True(problem.GetProperty("errors").TryGetProperty("RefreshToken", out _));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401ProblemDetails()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();
        var email = UniqueEmail("login");

        await RegisterAsync(client, email, "password123", "volunteer", cts.Token);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = "wrong-password" },
            cts.Token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Refresh_RotatesPair_And_OldTokenReuse_Returns401()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();
        var email = UniqueEmail("refresh");

        var registered = await RegisterAsync(client, email, "password123", "volunteer", cts.Token);
        var firstRefresh = registered.GetProperty("refreshToken").GetString()!;

        var refresh = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = firstRefresh },
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var rotated = await refresh.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        var secondRefresh = rotated.GetProperty("refreshToken").GetString()!;
        Assert.NotEqual(firstRefresh, secondRefresh);

        // Reuse of the rotated-out token must be rejected (reuse detection).
        var reuse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = firstRefresh },
            cts.Token);

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesToken_SoRefreshFails()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();
        var email = UniqueEmail("logout");

        var registered = await RegisterAsync(client, email, "password123", "volunteer", cts.Token);
        var refreshToken = registered.GetProperty("refreshToken").GetString()!;

        var logout = await client.PostAsJsonAsync(
            "/api/auth/logout",
            new { refreshToken },
            cts.Token);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken },
            cts.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/me", cts.Token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Volunteer_CannotRegisterOrganization_Returns403()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var registered = await RegisterAsync(client, UniqueEmail("vol"), "password123", "volunteer", cts.Token);
        UseAccessToken(client, registered.GetProperty("accessToken").GetString()!);

        var response = await client.PostAsJsonAsync(
            "/api/organizations",
            new
            {
                name = "Food Bank",
                type = "Nonprofit",
                causeTags = new[] { "Hunger" },
                location = "Dhaka",
                description = "Feeding the city.",
            },
            cts.Token);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrgAdmin_OrganizationLifecycle_PendingThenEditable()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var registered = await RegisterAsync(client, UniqueEmail("org"), "password123", "org_admin", cts.Token);
        UseAccessToken(client, registered.GetProperty("accessToken").GetString()!);

        var created = await client.PostAsJsonAsync(
            "/api/organizations",
            new
            {
                name = "Food Bank",
                type = "Nonprofit",
                causeTags = new[] { "Hunger" },
                location = "Dhaka",
                description = "Feeding the city.",
            },
            cts.Token);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var org = await created.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        var orgId = org.GetProperty("id").GetString()!;
        Assert.False(org.GetProperty("isVerified").GetBoolean());

        var mine = await client.GetAsync("/api/organizations/me", cts.Token);
        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);

        var updated = await client.PutAsJsonAsync(
            $"/api/organizations/{orgId}",
            new
            {
                name = "Food Bank Plus",
                type = "Nonprofit",
                causeTags = new[] { "Hunger", "Education" },
                location = "Dhaka",
                description = "Feeding and teaching the city.",
            },
            cts.Token);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var fetched = await client.GetAsync($"/api/organizations/{orgId}", cts.Token);
        var fetchedOrg = await fetched.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        Assert.Equal("Food Bank Plus", fetchedOrg.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Volunteer_ProfileCrud_WorksForOwnerOnly()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var registered = await RegisterAsync(client, UniqueEmail("profile"), "password123", "volunteer", cts.Token);
        UseAccessToken(client, registered.GetProperty("accessToken").GetString()!);

        var payload = new
        {
            skills = new[] { "Teaching" },
            availability = "Weekends",
            causes = new[] { "Education" },
            location = "Dhaka",
            bio = "Happy to help.",
        };

        var created = await client.PostAsJsonAsync("/api/volunteer-profiles/me", payload, cts.Token);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/volunteer-profiles/me", payload, cts.Token);
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);

        var updated = await client.PutAsJsonAsync(
            "/api/volunteer-profiles/me",
            new
            {
                skills = new[] { "Teaching", "Cooking" },
                availability = "Evenings",
                causes = new[] { "Education" },
                location = "Sylhet",
                bio = "Updated bio.",
            },
            cts.Token);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var fetched = await client.GetAsync("/api/volunteer-profiles/me", cts.Token);
        var profile = await fetched.Content.ReadFromJsonAsync<JsonElement>(cts.Token);
        Assert.Equal("Sylhet", profile.GetProperty("location").GetString());
        Assert.Equal(2, profile.GetProperty("skills").GetArrayLength());
    }

    [Fact]
    public async Task Google_WithoutConfiguration_Returns401()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/google",
            new { code = "any-code", role = "volunteer" },
            cts.Token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
