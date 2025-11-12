using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Services.Messages;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
