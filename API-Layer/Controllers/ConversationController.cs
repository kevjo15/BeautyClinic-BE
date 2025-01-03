using Application.Features.Conversations.Commands;
using Application.Features.Conversations.Queries;
using Application.Features.MessageCommands.SendMessage;
using ApplicationLayer.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConversationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAllConversations")]
        public async Task<IActionResult> GetAllConversations()
        {
            var query = new GetAllConversationsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("GetConversationById/{conversationId}")]
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

        [HttpPost("CreateConversation")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetConversationById), new { conversationId = result }, command);
        }

        [HttpGet("{conversationId}/GetMessagesForConversation")]
        public async Task<IActionResult> GetMessagesForConversation(Guid conversationId)
        {
            var query = new GetMessagesForConversationQuery { ConversationId = conversationId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{conversationId}/SendMessage")]
        public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageDTO messageDto)
        {
            var command = new SendMessageCommand { MessageDto = messageDto };
            command.MessageDto.ConversationId = conversationId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
