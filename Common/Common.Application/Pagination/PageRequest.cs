using Common.Domain.Results;

namespace Common.Application.Pagination;

public sealed record PageRequest(int Page, int Size)
{
    public const int MinPage = 1;
    public const int MinSize = 1;
    public const int MaxSize = 100;
    public const int DefaultSize = 20;

    public int Skip => (Page - MinPage) * Size;

    public Result Validate()
    {
        if (Page < MinPage)
        {
            return Result.Failure(PaginationErrors.PageOutOfRange);
        }

        return Size is < MinSize or > MaxSize
            ? Result.Failure(PaginationErrors.SizeOutOfRange)
            : Result.Success();
    }
}
