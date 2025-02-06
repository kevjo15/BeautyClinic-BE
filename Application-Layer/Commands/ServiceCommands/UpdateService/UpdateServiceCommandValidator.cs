using FluentValidation;
using Application_Layer.Validators.ValidationExtensions;

namespace Application_Layer.Commands.ServiceCommands.UpdateService
{
    public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
    {
        public UpdateServiceCommandValidator()
        {
            RuleFor(command => command.ServiceDto.Name)
                .MustBeValidServiceName();

            RuleFor(command => command.ServiceDto.Description)
                .MustBeValidDescription();

            RuleFor(command => command.ServiceDto.Duration)
                .MustBeValidDuration();

            RuleFor(command => command.ServiceDto.Price)
                .MustBeValidPrice();
        }
    }
} 