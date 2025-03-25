using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record GetLoggedInUserQuery : IQuery<UserResponse>;
