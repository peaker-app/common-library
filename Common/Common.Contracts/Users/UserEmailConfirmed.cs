using Common.Contracts.Abstractions;

namespace Common.Contracts.Users;

public sealed record UserEmailConfirmed : IntegrationEvent
{
    public required Guid UserId { get; init; }
}
