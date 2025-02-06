using FluentValidation;
using Application_Layer.Validators.ValidationExtensions;

namespace Application_Layer.Commands.CategoryCommands.UpdateCategory
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(command => command.CategoryDto.Name)
                .ValidCategoryName();
        }
    }
} 