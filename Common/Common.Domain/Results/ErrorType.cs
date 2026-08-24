namespace Common.Domain.Results;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Unauthorized = 2,
    NotFound = 3,
    Conflict = 4,
    Forbidden = 5,
    Unavailable = 6,
    TooManyRequests = 7
}
