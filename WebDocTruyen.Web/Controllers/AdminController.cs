using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Application.Services;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Web.Models;

namespace WebDocTruyen.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IStoryRepository _storyRepo;
        private readonly StoryService _storyService;

        public AdminController(IUserRepository userRepo, IStoryRepository storyRepo, StoryService storyService)
        {
            _userRepo = userRepo;
            _storyRepo = storyRepo;
            _storyService = storyService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = _userRepo.GetAll().ToList();
            var stories = (await _storyRepo.GetAllAsync()).ToList();
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalAdmins = users.Count(u => u.Role == "Admin");
            ViewBag.TotalEditors = users.Count(u => u.Role == "Editor");
            ViewBag.TotalNormalUsers = users.Count(u => u.Role == "User");
            ViewBag.RecentUsers = users.OrderByDescending(u => u.LastLogin)
                                            .Take(5).Select(UserMapper.ToDto).ToList();
            ViewBag.TotalStories = stories.Count;
            ViewBag.TotalChapters = stories.Sum(s => s.Chapters.Count);
            return View();
        }

        // ManageUsers → List<UserDto>
        public IActionResult ManageUsers(string? keyword, string? role, int page = 1, int pageSize = 15)
        {
            var users = _userRepo.GetAll().AsEnumerable();
            if (!string.IsNullOrEmpty(keyword))
                users = users.Where(u => u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                      || u.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);

            var list = users.OrderByDescending(u => u.LastLogin).ToList();
            int total = list.Count;
            int pages = (int)Math.Ceiling((double)total / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, pages));

            ViewBag.Keyword = keyword;
            ViewBag.RoleFilter = role;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = pages;
            ViewBag.TotalItems = total;
            ViewBag.PageSize = pageSize;
            ViewBag.BaseUrl = Url.Action("ManageUsers") + BuildQs(new { keyword, role, pageSize });

            return View(list.Skip((page - 1) * pageSize).Take(pageSize)
                           .Select(UserMapper.ToDto).ToList());
        }

        // ManageStories → List<StoryDto>
        public async Task<IActionResult> ManageStories(string? keyword, string? status, int page = 1, int pageSize = 15)
        {
            var all = await _storyService.GetAllStoriesAsync();
            var stories = all.AsEnumerable();
            if (!string.IsNullOrEmpty(keyword))
                stories = stories.Where(s => s.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                          || s.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(status))
                stories = stories.Where(s => s.Status == status);

            var list = stories.OrderByDescending(s => s.CreatedAt).ToList();
            int total = list.Count;
            int pages = (int)Math.Ceiling((double)total / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, pages));

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = pages;
            ViewBag.TotalItems = total;
            ViewBag.PageSize = pageSize;
            ViewBag.BaseUrl = Url.Action("ManageStories") + BuildQs(new { keyword, status, pageSize });

            return View(list.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddUser(string Username, string Email, string Password, string Role)
        {
            if (_userRepo.GetByEmail(Email) != null)
            { TempData["Error"] = "Email đã tồn tại!"; return RedirectToAction("ManageUsers"); }
            _userRepo.Add(new User
            {
                Username = Username,
                Email = Email,
                Role = Role,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
            });
            TempData["Success"] = "Thêm tài khoản thành công!";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var currentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == currentId) { TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập!"; return RedirectToAction("ManageUsers"); }
            var user = _userRepo.GetById(id);
            if (user != null) _userRepo.Delete(user);
            TempData["Success"] = "Xóa tài khoản thành công!";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangeRole(int id, string role)
        {
            var currentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == currentId) { TempData["Error"] = "Không thể đổi role của chính mình!"; return RedirectToAction("ManageUsers"); }
            var user = _userRepo.GetById(id);
            if (user != null) { user.Role = role; _userRepo.Update(user); }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStory(int id)
        {
            await _storyRepo.DeleteAsync(id);
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stories", id.ToString());
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            TempData["Success"] = "Đã xóa truyện!";
            return RedirectToAction("ManageStories");
        }

        private static string BuildQs(object? f)
        {
            if (f == null) return "";
            var parts = f.GetType().GetProperties()
                .Select(p => (p.Name, Val: p.GetValue(f)?.ToString()))
                .Where(x => !string.IsNullOrEmpty(x.Val))
                .Select(x => $"{x.Name}={Uri.EscapeDataString(x.Val!)}");
            var qs = string.Join("&", parts);
            return qs.Length > 0 ? "?" + qs : "";
        }
    }
}