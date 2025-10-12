using MediatR;
using Ardalis.Result;

namespace AppTemplate.Application.Services.Messages;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
