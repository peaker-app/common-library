using Common.Domain.Results;
using MediatR;

namespace Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
