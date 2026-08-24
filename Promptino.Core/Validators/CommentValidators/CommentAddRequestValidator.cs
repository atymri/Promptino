using FluentValidation;
using Promptino.Core.DTOs;

namespace Promptino.Core.Validators.CommentValidators;

public class CommentAddRequestValidator : AbstractValidator<CommentAddRequest>
{
    public CommentAddRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("متن نظر نمی‌تواند خالی باشد")
            .MinimumLength(2).WithMessage("متن نظر باید حداقل ۲ کاراکتر باشد")
            .MaximumLength(500).WithMessage("متن نظر نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد");
    }
}
