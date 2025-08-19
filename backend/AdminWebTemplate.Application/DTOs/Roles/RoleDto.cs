using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminWebTemplate.Application.DTOs.Roles
{
    public sealed class RoleDto
    {
        public string Id { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public int UsersCount { get; set; }
    }
}
