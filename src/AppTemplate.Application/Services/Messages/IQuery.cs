using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Services.Messages;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
