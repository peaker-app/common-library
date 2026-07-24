using Common.Application.Pagination;

namespace Common.API.Responses;

public static class PagedResponseExtensions
{
    public static PagedResponse<T> ToPagedResponse<T>(this PagedResult<T> result) =>
        new(result.Items, result.Page, result.Size, result.TotalCount, result.TotalPages);
}
