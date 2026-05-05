using Application_Layer.DTOs;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.ConversationCommands.CreateConversation
{
    public class CreateConversationCommand : IRequest<ConversationDTO>
    {
        public ConversationModel Conversation { get; set; } = new();
    }
}
