using Common.Domain.Results;

namespace Common.Application.Pagination;

public static class PaginationErrors
{
    public static readonly Error PageOutOfRange = Error.Validation(
        "Pagination.PageOutOfRange",
        $"El número de página debe ser mayor o igual que {PageRequest.MinPage}.");

    public static readonly Error SizeOutOfRange = Error.Validation(
        "Pagination.SizeOutOfRange",
        $"El tamaño de página debe estar entre {PageRequest.MinSize} y {PageRequest.MaxSize}.");
}
