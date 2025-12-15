using FluentValidation;

namespace Application_Layer.Commands.BookingCommands.AssignEmployee
{
    public class AssignEmployeeCommandValidator : AbstractValidator<AssignEmployeeCommand>
    {
        public AssignEmployeeCommandValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty().WithMessage("BookingId is required.");

            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("EmployeeId is required.");
        }
    }
}
