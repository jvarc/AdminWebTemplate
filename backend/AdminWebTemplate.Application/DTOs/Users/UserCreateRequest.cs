using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminWebTemplate.Application.DTOs.Users
{
    public class UserCreateRequest
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
}
