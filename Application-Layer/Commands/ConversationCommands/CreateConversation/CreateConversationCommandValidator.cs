using FluentValidation;

namespace Application_Layer.Commands.ConversationCommands.CreateConversation
{
    public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationCommandValidator()
        {
            RuleFor(x => x.Conversation)
                .NotNull()
                .WithMessage("Conversation data is required.");

            RuleFor(x => x.Conversation.ParticipantIds)
                .NotNull()
                .NotEmpty()
                .WithMessage("A conversation must have at least one participant.");
        }
    }
}
