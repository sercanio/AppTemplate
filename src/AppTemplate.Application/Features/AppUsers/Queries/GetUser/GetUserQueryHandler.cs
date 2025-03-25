using Ardalis.Result;
using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetUser;

public sealed class GetUserQueryHandler(IAppUsersRepository userRepository) : IQueryHandler<GetUserQuery, GetUserQueryResponse>
{
    private readonly IAppUsersRepository _userRepository = userRepository;

    public async Task<Result<GetUserQueryResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(
            predicate: u => u.Id == request.UserId && u.DeletedOnUtc == null,
            includeSoftDeleted: false,
            include: [
                u => u.Roles,
                u => u.IdentityUser]);

        if (user is null)
        {
            return Result<GetUserQueryResponse>.NotFound(AppUserErrors.NotFound.Name);
        }

        List<GetRoleByIdQueryResponse> mappedRoles = user.Roles.Select(role =>
            new GetRoleByIdQueryResponse(role.Id, role.Name.Value, role.IsDefault.Value)).ToList();

        GetUserQueryResponse response = new(
            user.Id,
            user.IdentityUser.Email!,
            user.IdentityUser.UserName!,
            new Collection<GetRoleByIdQueryResponse>(mappedRoles));

        return Result.Success(response);
    }
}
