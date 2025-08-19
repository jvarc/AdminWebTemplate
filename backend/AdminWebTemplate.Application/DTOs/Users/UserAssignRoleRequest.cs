using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminWebTemplate.Application.DTOs.Users
{
    public class UserAssignRoleRequest
    {
        public string UserId { get; set; } = ""; 
        public string RoleName { get; set; } = "";
    }
}
