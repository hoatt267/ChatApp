using IdentityService.Application.DTOs;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Users.Commands;
using IdentityService.Application.Features.Users.Commands.CreateUser;
using IdentityService.Application.Wrappers;
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
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful."));
    }
}