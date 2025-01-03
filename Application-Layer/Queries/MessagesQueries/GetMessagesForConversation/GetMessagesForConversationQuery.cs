using MediatR;
using System;
using System.Collections.Generic;
using Domain_Layer.Models;

namespace Application.Features.Conversations.Queries
{
    public class GetMessagesForConversationQuery : IRequest<List<MessageModel>>
    {
        public Guid ConversationId { get; set; }
    }
}
