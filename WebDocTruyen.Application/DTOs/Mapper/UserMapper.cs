using WebDocTruyen.Application.DTOs.User;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(User u) => new()
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role ?? "User",
            CreatedAt = u.CreatedAt,
            LastLogin = u.LastLogin
        };

        public static UserProfileDto ToProfileDto(User u) => new()
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email
        };
    }
}