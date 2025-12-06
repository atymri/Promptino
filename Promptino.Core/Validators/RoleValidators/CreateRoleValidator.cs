using FluentValidation;
using Promptino.Core.DTOs;

namespace Promptino.Core.Validators.RoleValidators;

public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("نام نقش نمی‌تواند خالی باشد.")
            .MaximumLength(100).WithMessage("نام نقش نمی‌تواند بیش از 100 کاراکتر باشد.");

        RuleFor(x => x.Details)
            .MaximumLength(500).WithMessage("جزئیات نقش نمی‌تواند بیش از 500 کاراکتر باشد.");
    }
}
