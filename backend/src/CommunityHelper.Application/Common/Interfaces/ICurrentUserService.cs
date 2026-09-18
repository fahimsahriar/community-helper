namespace CommunityHelper.Application.Common.Interfaces;

/// <summary>
/// Authenticated user information extracted from JWT claims.
/// Implemented in Infrastructure from HttpContext; handlers never touch HttpContext.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
