using AdminWebTemplate.Application.DTOs.Roles;

namespace AdminWebTemplate.Application.DTOs.Users
{
    public class UserRoleDto
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public bool IsInactive { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
