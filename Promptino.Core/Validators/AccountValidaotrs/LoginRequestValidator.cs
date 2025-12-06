using FluentValidation;
using Promptino.Core.DTOs;
using Promptino.Core.Validators.CustomValidators;

namespace Promptino.Core.Validators.AccountValidaotrs;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(req => req.Email)
         .NotEmpty().WithMessage("ایمیل نمیتواند خالی باشد")
         .EmailAddress().WithMessage("فرمت ایمیل وارد شده معتبر نیست")
         .Must(CustomRequestValidator.BeValidEmail).WithMessage("ایمیل وارد شده متعلق به دامنه معتبری نیست");

        RuleFor(req => req.Password)
            .NotEmpty().WithMessage("رمز عبور نمیتواند خالی باشد");

        RuleFor(req => req.ConfirmPassword)
            .NotEmpty().WithMessage("لطفا رمز عبور را تایید کنید")
            .Equal(req => req.Password).WithMessage("رمز عبور و تکرار آن یکسان نیستند");

    }
}
