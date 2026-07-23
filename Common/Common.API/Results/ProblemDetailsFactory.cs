using Common.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Results;

internal static class ProblemDetailsFactory
{
    public static int StatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    public static ProblemDetails Create(Error error) => error is ValidationError validationError
        ? CreateValidationProblem(validationError)
        : new ProblemDetails
        {
            Status = StatusCode(error.Type),
            Title = error.Code,
            Detail = error.Description
        };

    private static ValidationProblemDetails CreateValidationProblem(ValidationError validationError) =>
        new(ToErrors(validationError))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };

    private static Dictionary<string, string[]> ToErrors(ValidationError validationError) =>
        validationError.Errors
            .GroupBy(error => error.Code, error => error.Description)
            .ToDictionary(group => group.Key, group => group.ToArray());
}
