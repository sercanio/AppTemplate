﻿using FluentValidation;

namespace EcoFind.Application.Features.Roles.Commands.Create;

public sealed class CreateRoleValidationhandler : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidationhandler()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(25);
    }
}
