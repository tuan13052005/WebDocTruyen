using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IFavoriteService _favoriteService;

        public AccountController(IUserRepository userRepo, IFavoriteService favoriteService)
        {
            _userRepo = userRepo;
            _favoriteService = favoriteService;
        }

        // ── Login ──────────────────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password, string? returnUrl = null)
        {
            var user = _userRepo.GetByEmail(Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            user.LastLogin = DateTime.Now;
            _userRepo.Update(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name,           user.Username),
                new(ClaimTypes.Email,          user.Email),
                new(ClaimTypes.Role,           user.Role ?? "User"),
            };

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Editor" => RedirectToAction("Dashboard", "Editor"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ── Logout ─────────────────────────────────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ── Register ───────────────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public IActionResult Register(string Username, string Email, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            if (_userRepo.GetByEmail(Email) != null)
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View();
            }

            _userRepo.Add(new User
            {
                Username = Username,
                Email = Email,
                Role = "User",
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
            });

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ── Access Denied ──────────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // ── Profile ────────────────────────────────────────────────────────

        [Authorize]
        public IActionResult Profile()
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login");
            return View(UserMapper.ToDto(user));
        }

        // ── My Favorites ───────────────────────────────────────────────────

        [Authorize]
        public async Task<IActionResult> MyFavorites()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var list = await _favoriteService.GetMyFavoritesAsync(userId);
            return View(list);
        }

        // ── Settings ───────────────────────────────────────────────────────

        [Authorize]
        public IActionResult Settings()
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login");
            return View(UserMapper.ToProfileDto(user));
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(WebDocTruyen.Application.DTOs.User.UserProfileDto updatedUser)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login");

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            _userRepo.Update(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name,           user.Username),
                new(ClaimTypes.Email,          user.Email),
                new(ClaimTypes.Role,           user.Role ?? "User"),
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            ViewBag.Success = "Cập nhật thành công!";
            return View(UserMapper.ToProfileDto(user));
        }

        // ── Change Password ────────────────────────────────────────────────

        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login");

            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng!";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            _userRepo.Update(user);

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

        // ── Helper ─────────────────────────────────────────────────────────

        private WebDocTruyen.Domain.Entities.User? CurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            return email == null ? null : _userRepo.GetByEmail(email);
        }
    }
}