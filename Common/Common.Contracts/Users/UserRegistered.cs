using Common.Contracts.Abstractions;

namespace Common.Contracts.Users;

public sealed record UserRegistered : IntegrationEvent
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }
}
