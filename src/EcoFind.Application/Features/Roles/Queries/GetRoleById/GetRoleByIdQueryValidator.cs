using FluentValidation;

namespace EcoFind.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdQueryValidator()
    {
        RuleFor(r => r.RoleId).NotEmpty();
    }
}