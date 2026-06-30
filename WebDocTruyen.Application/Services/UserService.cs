using BCrypt.Net;
using WebDocTruyen.Application.DTOs.User;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public IEnumerable<UserDto> GetAll() =>
            _userRepo.GetAll().Select(UserMapper.ToDto);

        public UserDto? GetById(int id)
        {
            var user = _userRepo.GetById(id);
            return user == null ? null : UserMapper.ToDto(user);
        }

        public UserDto? GetByEmail(string email)
        {
            var user = _userRepo.GetByEmail(email);
            return user == null ? null : UserMapper.ToDto(user);
        }

        public int Register(string username, string email, string password)
        {
            if (_userRepo.GetByEmail(email) != null)
                throw new InvalidOperationException("Email đã được sử dụng.");

            var user = new User
            {
                Username = username.Trim(),
                Email = email.Trim().ToLower(),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _userRepo.Add(user);
            return user.UserId;
        }

        public UserDto? Authenticate(string email, string password)
        {
            var user = _userRepo.GetByEmail(email.Trim().ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            user.LastLogin = DateTime.UtcNow;
            _userRepo.Update(user);
            return UserMapper.ToDto(user);
        }

        public bool UpdateProfile(int userId, string username, string email)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return false;

            // Kiểm tra email trùng với user khác
            var existingByEmail = _userRepo.GetByEmail(email.Trim().ToLower());
            if (existingByEmail != null && existingByEmail.UserId != userId)
                throw new InvalidOperationException("Email đã được sử dụng bởi tài khoản khác.");

            user.Username = username.Trim();
            user.Email = email.Trim().ToLower();
            _userRepo.Update(user);
            return true;
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _userRepo.Update(user);
            return true;
        }

        public bool ChangeRole(int userId, string role)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return false;

            user.Role = role;
            _userRepo.Update(user);
            return true;
        }

        public bool Delete(int userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return false;

            _userRepo.Delete(user);
            return true;
        }
    }
}