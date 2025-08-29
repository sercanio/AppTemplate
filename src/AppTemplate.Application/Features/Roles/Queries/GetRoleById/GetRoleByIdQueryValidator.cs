using FluentValidation;

namespace AppTemplate.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
  public GetRoleByIdQueryValidator()
  {
    RuleFor(r => r.RoleId).NotEmpty();
  }
}