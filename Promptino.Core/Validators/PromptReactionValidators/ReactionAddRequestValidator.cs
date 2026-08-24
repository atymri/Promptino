using FluentValidation;
using Promptino.Core.DTOs;

namespace Promptino.Core.Validators.PromptReactionValidators;

public class ReactionAddRequestValidator : AbstractValidator<ReactionAddRequest>
{
    public ReactionAddRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع واکنش نامعتبر است");
    }
}
