using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace EcoFind.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record GetLoggedInUserQuery : IQuery<UserResponse>;
