namespace UserService.Application.DTOs.Response
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }
}