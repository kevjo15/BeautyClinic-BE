using Application_Layer.DTOs;
using Domain_Layer.Models;
using MediatR;

namespace Application.Features.Conversations.Commands
{
    public class CreateConversationCommand : IRequest<ConversationDTO>
    {
        public ConversationModel Conversation { get; set; }
    }
}
