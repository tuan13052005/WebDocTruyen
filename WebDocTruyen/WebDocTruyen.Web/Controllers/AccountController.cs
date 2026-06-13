using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    // Trang Account không yêu cầu đăng nhập (AllowAnonymous trên từng action)
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;

        public AccountController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password, string? returnUrl = null)
        {
            var user = _userRepo.GetByEmail(Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Cập nhật LastLogin
            user.LastLogin = DateTime.Now;
            _userRepo.Update(user);

            // Tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role ?? "User"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            // Redirect theo role
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Hỗ trợ GET logout từ navbar link (redirect về POST)
        [AllowAnonymous]
        public IActionResult LogoutGet() => View("Logout");

        // ── Register ───────────────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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

            var newUser = new User
            {
                Username = Username,
                Email = Email,
                Role = "User",
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
            };

            _userRepo.Add(newUser);
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
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _userRepo.GetByEmail(email!);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        // ── Settings ───────────────────────────────────────────────────────

        [Authorize]
        public IActionResult Settings()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _userRepo.GetByEmail(email!);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(User updatedUser)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _userRepo.GetByEmail(email!);
            if (user == null) return RedirectToAction("Login");

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            _userRepo.Update(user);

            // Cập nhật lại cookie với thông tin mới
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role ?? "User"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            ViewBag.Success = "Cập nhật thành công!";
            return View(user);
        }

        // ── Change Password ────────────────────────────────────────────────

        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _userRepo.GetByEmail(email!);
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
    }
}