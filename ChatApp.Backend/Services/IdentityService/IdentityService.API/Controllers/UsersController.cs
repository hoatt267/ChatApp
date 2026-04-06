using System.Security.Claims;
using IdentityService.Application.DTOs;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.RefreshToken;
using IdentityService.Application.Features.Users.Commands;
using ChatApp.Shared.Wrappers;
using ChatApp.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Shared.Extensions;
using IdentityService.Application.Features.Users.Commands.UploadAvatar;
using IdentityService.Application.Features.Users.Queries.GetUserById;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserCommand command)
    {
        // Gửi Command vào Pipeline. ValidationBehavior sẽ tự động chạy trước khi vào Handler.
        var result = await _mediator.Send(command);

        // Trả về ApiResponse chuẩn
        return Ok(ApiResponse<UserResponseDto>.Ok(result, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        // Lấy Token từ két sắt Cookie do trình duyệt (hoặc Postman) gửi lên
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            throw new BadRequestException("Refresh token is required.");

        var command = new RefreshTokenCommand { Token = refreshToken };
        var result = await _mediator.Send(command);

        // Đổi két sắt mới chứa Token mới
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            // 1. Hủy token trong Database
            await _mediator.Send(new LogoutCommand { RefreshToken = refreshToken });

            // 2. Xóa Két sắt Cookie trên trình duyệt
            Response.Cookies.Delete("refreshToken");
        }

        return Ok(ApiResponse<object>.Ok(null, "Logged out successfully."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.GetUserId();
        var query = new GetUserByIdQuery(userId);
        var userProfile = await _mediator.Send(query);

        return Ok(ApiResponse<object>.Ok(userProfile, "Profile retrieved successfully."));
    }

    [HttpPost("avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        // 1. Kiểm tra file hợp lệ
        if (file == null || file.Length == 0)
            throw new BadRequestException("No file uploaded.");

        if (file.Length > 5 * 1024 * 1024) // Giới hạn 5MB
            throw new BadRequestException("File size exceeds 5MB limit.");

        // 2. Lấy ID người dùng và luồng dữ liệu file
        var currentUserId = User.GetUserId();
        using var stream = file.OpenReadStream();

        var command = new UploadAvatarCommand(currentUserId, stream, file.FileName, file.ContentType);

        // 3. Thực thi Command
        var newAvatarUrl = await _mediator.Send(command);

        return Ok(ApiResponse<string>.Ok(newAvatarUrl, "Avatar uploaded successfully."));
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true,   // Chỉ gửi qua HTTPS
            SameSite = SameSiteMode.Strict
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}