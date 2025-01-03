using MediatR;
using System;
using Domain_Layer.Models; // Ensure this is included

namespace Application.Features.Conversations.Queries
{
    public class GetConversationByIdQuery : IRequest<ConversationModel>
    {
        public Guid ConversationId { get; set; }
    }
}
