using Common.Domain.Abstractions;
using FluentAssertions;
using Xunit;

namespace Common.Domain.UnitTests.Abstractions;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_WithTheSameTypeAndComponents_IsTrue()
    {
        Code left = new("PK");
        Code right = new("PK");

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentComponents_IsFalse()
    {
        Code left = new("PK");
        Code right = new("MB");

        left.Equals(right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithTheSameComponentsButADifferentType_IsFalse()
    {
        Code code = new("PK");
        Label label = new("PK");

        code.Equals(label).Should().BeFalse();
        label.Equals(code).Should().BeFalse();
        (code == label).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    public void Equals_WithAMissingValue_IsFalse(ValueObject? missing)
    {
        Code code = new("PK");

        code.Equals(missing).Should().BeFalse();
        (code == missing).Should().BeFalse();
        (missing == code).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithAnUnrelatedObject_IsFalse() =>
        new Code("PK").Equals("PK").Should().BeFalse();

    private sealed class Code(string value) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return value;
        }
    }

    private sealed class Label(string value) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return value;
        }
    }
}
