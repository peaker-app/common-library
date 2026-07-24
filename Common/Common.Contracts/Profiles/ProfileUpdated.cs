using Common.Contracts.Abstractions;

namespace Common.Contracts.Profiles;

public sealed record ProfileUpdated : IntegrationEvent
{
    public required Guid ProfileId { get; init; }

    public required Guid UserId { get; init; }

    public required string DisplayName { get; init; }

    public required string Slug { get; init; }

    public required string Visibility { get; init; }
}
