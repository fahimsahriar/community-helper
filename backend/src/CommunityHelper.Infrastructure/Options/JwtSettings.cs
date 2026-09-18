namespace CommunityHelper.Infrastructure.Options;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CommunityHelper";
    public string Audience { get; set; } = "CommunityHelper";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
