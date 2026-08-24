using Common.Infrastructure.Persistence.Outbox;
using FluentAssertions;
using Xunit;

namespace Common.Infrastructure.UnitTests.Persistence.Outbox;

public sealed class OutboxBacklogTests
{
    private static readonly OutboxOptions Options = new() { PendingAgeThreshold = TimeSpan.FromMinutes(5) };

    [Fact]
    public void ExceedsThreshold_WithAnEmptyBacklog_IsFalse() =>
        OutboxBacklog.Empty.ExceedsThreshold(Options).Should().BeFalse();

    [Fact]
    public void ExceedsThreshold_WithMessagesYoungerThanTheThreshold_IsFalse() =>
        new OutboxBacklog(5, 0, TimeSpan.FromMinutes(4)).ExceedsThreshold(Options).Should().BeFalse();

    [Fact]
    public void ExceedsThreshold_WithAMessageOlderThanTheThreshold_IsTrue() =>
        new OutboxBacklog(1, 0, TimeSpan.FromMinutes(6)).ExceedsThreshold(Options).Should().BeTrue();

    [Fact]
    public void ExceedsThreshold_WithAParkedMessage_IsTrueRegardlessOfAge() =>
        new OutboxBacklog(1, 1, TimeSpan.Zero).ExceedsThreshold(Options).Should().BeTrue();
}
