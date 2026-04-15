using ChatApp.Shared.Extensions;
using ChatApp.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Profiles.Commands.AcceptFriendRequest;
using UserService.Application.Features.Profiles.Commands.SendFriendRequest;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FriendsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] SendRequestDto dto)
        {
            var currentUserId = User.GetUserId();
            var command = new SendFriendRequestCommand(currentUserId, dto.TargetUserId);

            var result = await _mediator.Send(command);

            return Ok(ApiResponse<FriendshipDto>.Ok(result, "Friend request sent successfully."));
        }

        [HttpPost("requests/{friendshipId}/accept")]
        public async Task<IActionResult> AcceptRequest(Guid friendshipId)
        {
            var currentUserId = User.GetUserId();
            var command = new AcceptFriendRequestCommand(friendshipId, currentUserId);

            var result = await _mediator.Send(command);

            return Ok(ApiResponse<FriendshipDto>.Ok(result, "Friend request accepted."));
        }
    }
}