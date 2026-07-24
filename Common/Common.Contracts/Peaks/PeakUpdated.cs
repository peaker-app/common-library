using Common.Contracts.Abstractions;

namespace Common.Contracts.Peaks;

public sealed record PeakUpdated : IntegrationEvent
{
    public required Guid PeakId { get; init; }

    public required string Name { get; init; }

    public required int AltitudeM { get; init; }

    public string? CountryCode { get; init; }
}
