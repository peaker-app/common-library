using System.Reflection;
using Common.Application.Messaging;
using Common.Domain.Results;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Common.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    private static readonly MethodInfo GenericFailureMethod = typeof(Result)
        .GetMethods()
        .First(method => method is { Name: nameof(Result.Failure), IsGenericMethod: true });

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Error[] errors = await ValidateAsync(request, cancellationToken);

        return errors.Length == 0
            ? await next()
            : CreateFailureResult(new ValidationError(errors));
    }

    private async Task<Error[]> ValidateAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return [];
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        return [.. results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))];
    }

    private static TResponse CreateFailureResult(ValidationError validationError)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(validationError);
        }

        Type valueType = typeof(TResponse).GetGenericArguments()[0];
        object failure = GenericFailureMethod.MakeGenericMethod(valueType).Invoke(null, [validationError])!;

        return (TResponse)failure;
    }
}
