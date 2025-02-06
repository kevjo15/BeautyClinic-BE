using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.CategoryCommands.CreateCategory
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(command => command.CategoryName)
                .ValidCategoryName();
        }
    }
} 