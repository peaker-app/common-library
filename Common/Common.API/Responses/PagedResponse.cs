namespace Common.API.Responses;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    int TotalCount,
    int TotalPages);
