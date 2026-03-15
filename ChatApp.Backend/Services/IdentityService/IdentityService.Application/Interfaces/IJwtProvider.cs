using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}