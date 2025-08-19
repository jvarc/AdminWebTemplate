using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminWebTemplate.Application.DTOs.Users
{
    public sealed class UserUpdateRequest
    {
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public sealed class UserStatusRequest
    {
        public bool Active { get; set; }  
    }
}
