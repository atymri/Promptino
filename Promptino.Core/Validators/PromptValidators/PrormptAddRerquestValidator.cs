using FluentValidation;
using Promptino.Core.DTOs;

namespace Promptino.Core.Validators.PromptValidators;

public class PrormptAddRerquestValidator : AbstractValidator<PromptAddRequest>
{
    public PrormptAddRerquestValidator()
    {
        RuleFor(req => req.Title)
            .NotEmpty().WithMessage("عنوان پرامپت الزامی است")
            .MinimumLength(3).WithMessage("طول عنوان باید حداقل ۳ کاراکتر باشد")
            .MaximumLength(50).WithMessage("طول عنوان نباید بیشتر از ۵۰ کاراکتر باشد");

        RuleFor(req => req.Description)
            .NotEmpty().WithMessage("توضیحات پرامپت الزامی است")
            .MinimumLength(10).WithMessage("توضیحات باید حداقل ۱۰ کاراکتر باشد")
            .MaximumLength(150).WithMessage("توضیحات نباید بیشتر از ۱۵۰ کاراکتر باشد");

        RuleFor(req => req.Content)
            .NotEmpty().WithMessage("محتوای پرامپت الزامی است")
            .MinimumLength(30).WithMessage("محتوا باید حداقل ۳۰ کاراکتر باشد")
            .MaximumLength(2000).WithMessage("محتوا نباید بیشتر از 2000 کاراکتر باشد");
    }
}
