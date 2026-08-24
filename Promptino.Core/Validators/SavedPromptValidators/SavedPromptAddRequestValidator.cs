using FluentValidation;
using Promptino.Core.DTOs;

namespace Promptino.Core.Validators.SavedPromptValidators;

public class SavedPromptAddRequestValidator : AbstractValidator<SavedPromptAddRequest>
{
    public SavedPromptAddRequestValidator()
    {
        RuleFor(x => x.PromptID)
            .NotEmpty().WithMessage("شناسه پرامپت نامعتبر است");
    }
}
