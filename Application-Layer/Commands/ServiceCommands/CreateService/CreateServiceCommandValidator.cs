using FluentValidation;
using Application_Layer.DTO_s;
using Application_Layer.Validators.ValidationExtensions;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceCommandValidator()
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