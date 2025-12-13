using FluentValidation;
using Promptino.Core.DTOs;
using Promptino.Core.Validators.PromptValidators;

namespace Promptino.Core.Validators.CategoryValidators;

public class CategoryUpdateRequestValidatorr : AbstractValidator<CategoryUpdateRequest>
{
    public CategoryUpdateRequestValidatorr()
    {
        RuleFor(req => req.CategoryID)
            .NotEmpty().WithMessage("وارد کردن آیدی دسته بندی مورد نظر الزامی است")
            .Must(req => req != Guid.Empty).WithMessage("آیدی تصویر مورد نظر معتبر نیست");

        RuleFor(req => req.Title)
            .NotEmpty().WithMessage("وارد کردن عنوان دسته بندی الزامی است")
            .MinimumLength(3).WithMessage("حداقل طول نام دسته بندی 3 کاراکتر است")
            .MaximumLength(25).WithMessage("حداکثر طول نام دسته بندی 25 کاراکتر است");

        RuleFor(req => req.Description)
            .NotEmpty().WithMessage("وارد کردن توضیحات دسته بندی الزامی است")
            .MinimumLength(10).WithMessage("حداقل طول توضیحات 10 کاراکتر است")
            .MaximumLength(30).WithMessage("حداکثر طول توضیحات 30 کاراکتر است");

    }
}
