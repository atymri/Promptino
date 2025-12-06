using FluentValidation;
using Promptino.Core.DTOs; // فرض می‌کنیم AddUserToRoleDto اینجا هست
using Promptino.Core.Validators.CustomValidators;
using System;

namespace Promptino.Core.Validators.RoleValidators;

public class AddUserToRoleValidator : AbstractValidator<AddUserToRoleDto>
{
    public AddUserToRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("آیدی کاربر نمی‌تواند خالی باشد.")
            .Must(id => id != Guid.Empty).WithMessage("آیدی کاربر معتبر نیست");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("نام نقش نمی‌تواند خالی باشد.")
            .MaximumLength(100).WithMessage("نام نقش نمی‌تواند بیش از 100 کاراکتر باشد.");
    }

    
}
