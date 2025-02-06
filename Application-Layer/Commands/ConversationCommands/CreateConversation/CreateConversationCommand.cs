using Domain_Layer.Models;
using MediatR;

namespace Application.Features.Conversations.Commands
{
    public class CreateConversationCommand : IRequest<ConversationModel>
    {
        public ConversationModel Conversation { get; set; }
    }
}
