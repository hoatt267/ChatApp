using ChatApp.Shared.Extensions;
using ChatApp.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Friends.Commands.AcceptFriendRequest;
using UserService.Application.Features.Friends.Commands.BlockUser;
using UserService.Application.Features.Friends.Commands.RemoveFriendship;
using UserService.Application.Features.Friends.Commands.SendFriendRequest;
using UserService.Application.Features.Friends.Queries.GetFriends;
using UserService.Application.Features.Friends.Queries.GetSuggestedUsers;
using UserService.Application.Features.Friends.Queries.SearchUsers;
using UserService.Domain.Enums;

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

        [HttpPost("requests")]
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

        [HttpGet("accepted")]
        public async Task<IActionResult> GetFriends()
        {
            var currentUserId = User.GetUserId();
            var query = new GetFriendsQuery(currentUserId, FriendshipStatus.Accepted);
            var friends = await _mediator.Send(query);

            return Ok(ApiResponse<List<FriendProfileDto>>.Ok(friends, "Danh sách bạn bè."));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var currentUserId = User.GetUserId();
            var query = new GetFriendsQuery(currentUserId, FriendshipStatus.Pending);
            var requests = await _mediator.Send(query);

            return Ok(ApiResponse<List<FriendProfileDto>>.Ok(requests, "Danh sách lời mời kết bạn."));
        }

        // api for unfriend, or cancel pending request, or decline received request
        [HttpDelete("{targetUserId}")]
        public async Task<IActionResult> RemoveFriendship(Guid targetUserId, [FromQuery] FriendshipAction actionType)
        {
            var currentUserId = User.GetUserId();
            var command = new RemoveFriendshipCommand(currentUserId, targetUserId, actionType);

            await _mediator.Send(command);

            return Ok(ApiResponse<bool>.Ok(true, "Friendship record removed successfully."));
        }

        [HttpPost("block/{targetUserId}")]
        public async Task<IActionResult> BlockUser(Guid targetUserId)
        {
            var currentUserId = User.GetUserId();
            var command = new BlockUserCommand(currentUserId, targetUserId);

            await _mediator.Send(command);

            return Ok(ApiResponse<bool>.Ok(true, "User blocked successfully."));
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = User.GetUserId();
            var result = await _mediator.Send(new GetSuggestedUsersQuery(currentUserId, pageNumber, pageSize));
            return Ok(ApiResponse<PaginatedList<DiscoverUserDto>>.Ok(result));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string keyword, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var currentUserId = User.GetUserId();
            var result = await _mediator.Send(new SearchUsersQuery(currentUserId, keyword, pageNumber, pageSize));
            return Ok(ApiResponse<PaginatedList<DiscoverUserDto>>.Ok(result));
        }
    }
}