namespace Common.API.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Authority { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public IReadOnlyList<string> Audiences { get; init; } = [];

    public bool RequireHttpsMetadata { get; init; } = true;

    public TimeSpan MetadataAutomaticRefreshInterval { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan MetadataRefreshInterval { get; init; } = TimeSpan.FromSeconds(30);

    public TimeSpan MetadataLastKnownGoodLifetime { get; init; } = TimeSpan.FromHours(24);

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Authority)
        && !string.IsNullOrWhiteSpace(Issuer)
        && Audiences.Count > 0
        && Audiences.All(audience => !string.IsNullOrWhiteSpace(audience));
}
