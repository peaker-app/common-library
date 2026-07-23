namespace Common.Domain.Results;

public sealed record ValidationError(IReadOnlyCollection<Error> Errors)
    : Error("General.Validation", "One or more validation errors occurred.", ErrorType.Validation);
