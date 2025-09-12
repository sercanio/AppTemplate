using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record GetLoggedInUserQuery : IQuery<UserResponse>;