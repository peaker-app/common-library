using Common.Contracts.Abstractions;

namespace Common.Contracts.Ascents;

public sealed record AscentRegistered : IntegrationEvent
{
    public required Guid AscentId { get; init; }

    public required Guid UserId { get; init; }

    public required Guid PeakId { get; init; }

    public required string PeakName { get; init; }

    public required int PeakAltitudeM { get; init; }

    public required DateOnly AscentDate { get; init; }
}
