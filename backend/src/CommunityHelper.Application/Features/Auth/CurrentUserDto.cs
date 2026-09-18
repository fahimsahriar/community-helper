namespace CommunityHelper.Application.Features.Auth;

/// <summary>
/// Public identity of the authenticated user. Never includes hashes or tokens.
/// </summary>
public sealed record CurrentUserDto(string Id, string Email, string Role);
