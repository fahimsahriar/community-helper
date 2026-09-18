namespace CommunityHelper.Application.Features.Auth;

/// <summary>
/// JWT pair returned by register, login, refresh, and Google exchange.
/// </summary>
public sealed record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    CurrentUserDto User);
