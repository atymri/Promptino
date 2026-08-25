using FluentValidation;
using Promptino.Core.DTOs;

namespace Promptino.Core.Validators.ReportValidators;

public class PromptReportAddRequestValidator : AbstractValidator<PromptReportAddRequest>
{
    public PromptReportAddRequestValidator()
    {
        RuleFor(r => r.PromptID)
            .NotEmpty().WithMessage("آیدی پرامپت الزامی است");

        RuleFor(r => r.Reason)
            .NotEmpty().WithMessage("دلیل گزارش الزامی است")
            .MinimumLength(5).WithMessage("دلیل گزارش باید حداقل ۵ کاراکتر باشد")
            .MaximumLength(500).WithMessage("دلیل گزارش نباید بیشتر از ۵۰۰ کاراکتر باشد");
    }
}
