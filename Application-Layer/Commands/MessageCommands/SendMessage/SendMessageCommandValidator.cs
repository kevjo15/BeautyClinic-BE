using FluentValidation;

namespace Application_Layer.Commands.MessageCommands.SendMessage
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(x => x.MessageDto)
                .NotNull()
                .WithMessage("Message data is required.");

            RuleFor(x => x.MessageDto.ConversationId)
                .NotEmpty()
                .WithMessage("Conversation ID is required.");

            RuleFor(x => x.MessageDto.SenderId)
                .NotEmpty()
                .WithMessage("Sender ID is required.");

            RuleFor(x => x.MessageDto.Content)
                .NotEmpty()
                .WithMessage("Message content cannot be empty.")
                .MaximumLength(2000)
                .WithMessage("Message content cannot exceed 2000 characters.");
        }
    }
}
