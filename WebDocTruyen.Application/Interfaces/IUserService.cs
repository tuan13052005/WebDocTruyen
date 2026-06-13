using WebDocTruyen.Application.DTOs.User;

namespace WebDocTruyen.Application.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserDto> GetAll();
        UserDto? GetById(int id);
        UserDto? GetByEmail(string email);

        /// <summary>Đăng ký tài khoản mới. Trả về UserId mới hoặc ném exception nếu email đã tồn tại.</summary>
        int Register(string username, string email, string password);

        /// <summary>Xác thực đăng nhập. Trả về UserDto nếu hợp lệ, null nếu sai.</summary>
        UserDto? Authenticate(string email, string password);

        bool UpdateProfile(int userId, string username, string email);
        bool ChangePassword(int userId, string currentPassword, string newPassword);
        bool ChangeRole(int userId, string role);
        bool Delete(int userId);
    }
}