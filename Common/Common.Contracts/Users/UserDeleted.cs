using Common.Contracts.Abstractions;

namespace Common.Contracts.Users;

public sealed record UserDeleted : IntegrationEvent
{
    public required Guid UserId { get; init; }
}
