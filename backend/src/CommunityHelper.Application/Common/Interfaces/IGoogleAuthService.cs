namespace CommunityHelper.Application.Common.Interfaces;

/// <summary>
/// Verified Google identity returned by the OAuth code exchange.
/// </summary>
public sealed record GoogleUserInfo(string SubjectId, string Email);

/// <summary>
/// Exchanges a Google OAuth authorization code for a verified user identity.
/// </summary>
public interface IGoogleAuthService
{
    Task<GoogleUserInfo> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
}
