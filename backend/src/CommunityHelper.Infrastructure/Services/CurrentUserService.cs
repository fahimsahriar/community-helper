using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CommunityHelper.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CommunityHelper.Infrastructure.Services;

/// <summary>
/// Reads the authenticated user's identity from JWT claims.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        FindFirst(JwtRegisteredClaimNames.Sub)
        ?? FindFirst(ClaimTypes.NameIdentifier);

    public string? Email =>
        FindFirst(JwtRegisteredClaimNames.Email)
        ?? FindFirst(ClaimTypes.Email);

    public string? Role =>
        FindFirst(ClaimTypes.Role)
        ?? FindFirst("role");

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role) =>
        string.Equals(Role, role, StringComparison.Ordinal);

    private string? FindFirst(string claimType) =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
}
