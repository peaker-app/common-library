using Common.Infrastructure.Persistence.Outbox;
using FluentAssertions;
using Xunit;

namespace Common.Infrastructure.UnitTests.Persistence.Outbox;

public sealed class OutboxOptionsTests
{
    [Fact]
    public void IsValid_WithTheDefaults_IsTrue() => new OutboxOptions().IsValid().Should().BeTrue();

    [Fact]
    public void IsValid_WithoutAttempts_IsFalse() =>
        new OutboxOptions { MaxAttempts = 0 }.IsValid().Should().BeFalse();

    [Fact]
    public void IsValid_WithACapBelowTheBase_IsFalse() =>
        new OutboxOptions
        {
            RetryBackoffBase = TimeSpan.FromMinutes(5),
            RetryBackoffCap = TimeSpan.FromMinutes(1)
        }.IsValid().Should().BeFalse();

    [Fact]
    public void IsValid_WithoutAPollingInterval_IsFalse() =>
        new OutboxOptions { PollingInterval = TimeSpan.Zero }.IsValid().Should().BeFalse();
}
