namespace IdentityService.Application.DTOs.Responses
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = null!;
    }
}