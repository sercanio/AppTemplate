﻿using Ardalis.Result;
using EcoFind.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using EcoFind.Application.Features.AppUsers.Queries.GetLoggedInUser;
using EcoFind.Application.Repositories;
using EcoFind.Domain.AppUsers;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Collections.ObjectModel;

namespace Myrtus.Clarity.Application.Features.Users.Queries.GetAllUsersByRoleId
{
    public sealed class GetAllUsersByRoleIdQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersByRoleIdQuery, Result<IPaginatedList<GetAllUsersByRoleIdQueryResponse>>>
    {
        private readonly IAppUsersRepository _userRepository = userRepository;

        public async Task<Result<IPaginatedList<GetAllUsersByRoleIdQueryResponse>>> Handle(GetAllUsersByRoleIdQuery request, CancellationToken cancellationToken)
        {
            IPaginatedList<AppUser> users = await _userRepository.GetAllAsync(
                pageIndex: request.PageIndex,
                pageSize: request.PageSize,
                includeSoftDeleted: false,
                predicate: user => user.Roles.Any(role => role.Id == request.RoleId),
                include: [
                    user => user.Roles,
                    user => user.IdentityUser
                    ],
                cancellationToken: cancellationToken);

            List<GetAllUsersByRoleIdQueryResponse> mappedUsers = users.Items.Select(
                user => new GetAllUsersByRoleIdQueryResponse(
                    user.Id,
                    user.IdentityUser.Email!,
                    user.IdentityUser.Email!,
                    new Collection<LoggedInUserRolesDto>(
                        user.Roles.Where(
                            role => role.DeletedOnUtc == null).Select(
                            role => new LoggedInUserRolesDto(role.Id, role.Name.Value)).ToList())
            )).ToList();

            PaginatedList<GetAllUsersByRoleIdQueryResponse> paginatedList = new(
                mappedUsers,
                users.TotalCount,
                request.PageIndex,
                request.PageSize
            );

            return Result.Success<IPaginatedList<GetAllUsersByRoleIdQueryResponse>>(paginatedList);
        }
    }
}
