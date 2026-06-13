using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Application.DTOs.User
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
