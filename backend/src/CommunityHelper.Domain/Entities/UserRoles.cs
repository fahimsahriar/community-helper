namespace CommunityHelper.Domain.Entities;

/// <summary>
/// Role values used in JWT role claims and authorization policies.
/// Only Volunteer and OrgAdmin exist in Phase 1; any other role is denied
/// by role guards with 403.
/// </summary>
public static class UserRoles
{
    public const string Volunteer = "volunteer";
    public const string OrgAdmin = "org_admin";

    public static bool IsKnown(string? role) =>
        role is Volunteer or OrgAdmin;
}
