using Common.Contracts.Abstractions;

namespace Common.Contracts.Ascents;

public sealed record AscentDeleted : IntegrationEvent
{
    public required Guid AscentId { get; init; }

    public required Guid UserId { get; init; }

    public required Guid PeakId { get; init; }

    public required DateOnly AscentDate { get; init; }
}
