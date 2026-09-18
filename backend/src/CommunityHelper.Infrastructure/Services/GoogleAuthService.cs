using System.Net.Http.Json;
using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommunityHelper.Infrastructure.Services;

/// <summary>
/// Google OAuth code exchange. When <c>GoogleAuth</c> is configured, the
/// authorization code is exchanged at Google's token endpoint and the
/// returned ID token is validated at Google's tokeninfo endpoint (which
/// verifies signature, issuer, and expiry server-side); the audience is
/// then checked locally. Otherwise sign-in is rejected with 401 — Google
/// login must be configured before use.
/// </summary>
public sealed class GoogleAuthService(
    IOptions<GoogleAuthSettings> options,
    ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    private static readonly HttpClient Http = new();
    private readonly GoogleAuthSettings _settings = options.Value;

    public async Task<GoogleUserInfo> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId)
            || string.IsNullOrWhiteSpace(_settings.ClientSecret)
            || string.IsNullOrWhiteSpace(_settings.RedirectUri))
        {
            // Never log the code itself.
            logger.LogWarning("Google sign-in attempted but GoogleAuth is not configured.");
            throw new AuthenticationException("Google sign-in is not configured.");
        }

        using var response = await Http.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["redirect_uri"] = _settings.RedirectUri,
                ["grant_type"] = "authorization_code",
            }),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Google token exchange failed with {StatusCode}.", (int)response.StatusCode);
            throw new AuthenticationException("Google sign-in failed.");
        }

        var token = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.IdToken))
        {
            throw new AuthenticationException("Google sign-in failed.");
        }

        return await ValidateIdTokenAsync(token.IdToken, cancellationToken);
    }

    private async Task<GoogleUserInfo> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        GoogleTokenInfo? info;
        try
        {
            info = await Http.GetFromJsonAsync<GoogleTokenInfo>(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}",
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Google ID token validation failed.");
            throw new AuthenticationException("Google sign-in failed.");
        }

        if (info is null
            || !string.Equals(info.Audience, _settings.ClientId, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(info.SubjectId)
            || string.IsNullOrWhiteSpace(info.Email))
        {
            logger.LogWarning("Google ID token failed audience or claim validation.");
            throw new AuthenticationException("Google sign-in failed.");
        }

        return new GoogleUserInfo(info.SubjectId, info.Email);
    }

    private sealed record GoogleTokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("id_token")] string? IdToken);

    private sealed record GoogleTokenInfo(
        [property: System.Text.Json.Serialization.JsonPropertyName("aud")] string? Audience,
        [property: System.Text.Json.Serialization.JsonPropertyName("sub")] string? SubjectId,
        [property: System.Text.Json.Serialization.JsonPropertyName("email")] string? Email);
}
