using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace Common.Domain.UnitTests.Results;

public sealed class ResultTests
{
    private static readonly Error InvalidValue = Error.Validation("Test.Invalid", "The value is invalid.");

    [Fact]
    public void Success_WithoutAValue_HasNoError()
    {
        Result result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Success_WithAValue_ExposesIt()
    {
        Result<int> result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_WithoutAValue_KeepsTheError()
    {
        Result result = Result.Failure(InvalidValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvalidValue);
    }

    [Fact]
    public void Failure_WithAValue_ThrowsWhenTheValueIsRead()
    {
        Result<int> result = Result.Failure<int>(InvalidValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvalidValue);
        FluentActions.Invoking(() => result.Value).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_WithASuccessCarryingAnError_Throws() =>
        FluentActions.Invoking(() => new DerivedResult(true, InvalidValue))
            .Should().Throw<InvalidOperationException>();

    [Fact]
    public void Constructor_WithAFailureWithoutAnError_Throws() =>
        FluentActions.Invoking(() => new DerivedResult(false, Error.None))
            .Should().Throw<InvalidOperationException>();

    private sealed class DerivedResult(bool isSuccess, Error error) : Result(isSuccess, error);
}
