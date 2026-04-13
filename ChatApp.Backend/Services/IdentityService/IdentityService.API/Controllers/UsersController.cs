using IdentityService.Application.DTOs;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.RefreshToken;
using IdentityService.Application.Features.Users.Commands;
using ChatApp.Shared.Wrappers;
using ChatApp.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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