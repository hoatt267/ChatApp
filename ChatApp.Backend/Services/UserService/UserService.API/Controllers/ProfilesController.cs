using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Extensions;
using ChatApp.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Profiles.Commands.UploadAvatar;
using UserService.Application.Features.Profiles.Queries.GetProfile;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file uploaded.");

            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("File size exceeds 5MB limit.");

            var currentUserId = User.GetUserId();
            using var stream = file.OpenReadStream();

            var command = new UploadAvatarCommand(currentUserId, stream, file.FileName, file.ContentType);
            var newAvatarUrl = await _mediator.Send(command);

            return Ok(ApiResponse<string>.Ok(newAvatarUrl, "Avatar uploaded successfully."));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserId = User.GetUserId();
            var query = new GetProfileQuery(currentUserId);
            var profile = await _mediator.Send(query);

            return Ok(ApiResponse<ProfileDto>.Ok(profile, "Profile retrieved successfully."));
        }
    }
}