namespace Common.Application.Exceptions;

/// <summary>
/// Signals that a persisted aggregate changed between the read and the write of the same use case.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    private const string DefaultMessage = "The resource changed while the request was being processed.";

    public ConcurrencyConflictException()
        : base(DefaultMessage)
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConcurrencyConflictException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
