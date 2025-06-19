﻿using System.Security.Claims;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;

internal sealed class GetLoggedInUserQueryHandler : IQueryHandler<GetLoggedInUserQuery, UserResponse>
{
    private readonly IAppUsersRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetLoggedInUserQueryHandler(
        IAppUsersRepository userRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<UserResponse>> Handle(GetLoggedInUserQuery request, CancellationToken cancellationToken)
    {
        // Get the currently logged in user's Id from the claims
        string? userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Result<UserResponse>.NotFound("User not found in claims");
        }

        // Use the userId (which should be the same as IdentityUser.Id) to get the corresponding domain user.
        // Adjust the predicate if your domain user stores the identity user Id differently.
        var user = await _userRepository.GetAsync(
            predicate: u => u.IdentityId == userId,
            includeSoftDeleted: false,
            cancellationToken: cancellationToken, // if needed
            u => u.IdentityUser,
            u => u.Roles);


        if (user is null)
        {
            return Result<UserResponse>.NotFound(AppUserErrors.NotFound.Name);
        }

        List<LoggedInUserRolesDto> mappedRoles = user.Roles
            .Select(role => new LoggedInUserRolesDto(role.Id, role.Name.Value))
            .ToList();

        UserResponse userResponse = new()
        {
            Email = user.IdentityUser.Email!,
            UserName = user.IdentityUser.UserName!,
            Roles = mappedRoles,
            NotificationPreference = user.NotificationPreference,
            EmailConfirmed = user.IdentityUser.EmailConfirmed
        };

        return Result.Success(userResponse);
    }
}