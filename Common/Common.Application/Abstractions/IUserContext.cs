namespace Common.Application.Abstractions;

public interface IUserContext
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}
