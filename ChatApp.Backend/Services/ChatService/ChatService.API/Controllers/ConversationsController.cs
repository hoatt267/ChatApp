using ChatApp.Shared.Wrappers;
using ChatService.Application.DTOs;
using ChatService.Application.Features.Chats.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConversationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int limit = 50, [FromQuery] DateTime? before = null)
        {
            var query = new GetMessagesQuery(conversationId, limit, before);
            var resultDto = await _mediator.Send(query);

            var apiResponse = ApiResponse<IEnumerable<MessageDto>>.Ok(resultDto, "Messages retrieved successfully.");

            return Ok(apiResponse);
        }
    }
}