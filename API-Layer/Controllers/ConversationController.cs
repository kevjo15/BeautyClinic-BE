using Application_Layer.Commands.ConversationCommands.CreateConversation;
using Application_Layer.Commands.MessageCommands.SendMessage;
using Application_Layer.DTOs;
using Application_Layer.Queries.ConversationsQueries.GetAllConversations;
using Application_Layer.Queries.ConversationsQueries.GetConversationById;
using Application_Layer.Queries.MessagesQueries.GetMessagesForConversation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ConversationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConversations()
        {
            var query = new GetAllConversationsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversationById(Guid conversationId)
        {
            var query = new GetConversationByIdQuery { ConversationId = conversationId };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound("Conversation not found");
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetConversationById), new { conversationId = result.Id }, result);
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessagesForConversation(Guid conversationId)
        {
            var query = new GetMessagesForConversationQuery { ConversationId = conversationId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageDTO messageDto)
        {
            var command = new SendMessageCommand { MessageDto = messageDto };
            command.MessageDto.ConversationId = conversationId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
