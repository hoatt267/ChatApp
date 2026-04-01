using ChatApp.Shared.Domain;
using IdentityService.Domain.Enums;

namespace IdentityService.Domain.Entities
{
    public class Role : BaseEntity
    {
        public Role() { }
        public Role(RoleEnum name)
        {
            Name = name;
        }
        public RoleEnum Name { get; private set; } = RoleEnum.User;

        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    }
}