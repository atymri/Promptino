using FluentValidation;
using Promptino.Core.DTOs;
using Promptino.Core.Validators.CustomValidators;

namespace Promptino.Core.Validators.ImageValidators;

public class ImageUpdateRequestValidator : AbstractValidator<ImageUpdateRequest>
{
    public ImageUpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("آیدی تصویر نمی‌تواند خالی باشد")
            .Must(id => id != Guid.Empty).WithMessage("آیدی تصویر معتبر نیست");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان تصویر نمی‌تواند خالی باشد")
            .MaximumLength(100).WithMessage("عنوان تصویر نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد")
            .MinimumLength(3).WithMessage("عنوان تصویر نمی‌تواند کمتر از ۳ کاراکتر باشد");

        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("مسیر تصویر نمی‌تواند خالی باشد")
            .Must(CustomRequestValidator.BeAValidImagePath).WithMessage("مسیر تصویر معتبر نیست")
            .MaximumLength(500).WithMessage("مسیر تصویر نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد");

        RuleFor(x => x.GeneratedWith)
            .NotEmpty().WithMessage("ابزار تولید تصویر نمی‌تواند خالی باشد")
            .MaximumLength(50).WithMessage("ابزار تولید تصویر نمی‌تواند بیشتر از ۵۰ کاراکتر باشد");
    }
   
}