using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Results;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess ? new NoContentResult() : Problem(result.Error);

    public static IActionResult ToActionResult<TValue>(this Result<TValue> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : Problem(result.Error);

    public static IActionResult ToActionResult<TValue>(
        this Result<TValue> result,
        Func<TValue, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    private static ObjectResult Problem(Error error) => new(ProblemDetailsFactory.Create(error))
    {
        StatusCode = ProblemDetailsFactory.StatusCode(error.Type)
    };
}
