using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IStoryRepository _storyRepo;

        public AdminController(IUserRepository userRepo, IStoryRepository storyRepo)
        {
            _userRepo = userRepo;
            _storyRepo = storyRepo;
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = _userRepo.GetAll().ToList();
            var stories = (await _storyRepo.GetAllAsync()).ToList();
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalAdmins = users.Count(u => u.Role == "Admin");
            ViewBag.TotalEditors = users.Count(u => u.Role == "Editor");
            ViewBag.TotalNormalUsers = users.Count(u => u.Role == "User");
            ViewBag.RecentUsers = users.OrderByDescending(u => u.LastLogin).Take(5);
            ViewBag.TotalStories = stories.Count;
            ViewBag.TotalChapters = stories.Sum(s => s.Chapters.Count);
            return View();
        }

        // ── Manage Users (có phân trang + tìm kiếm) ───────────────────────
        public IActionResult ManageUsers(string? keyword, string? role,
                                         int page = 1, int pageSize = 15)
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
            ViewBag.BaseUrl = Url.Action("ManageUsers")
                + BuildQueryExcludePage(new { keyword, role, pageSize });

            return View(list.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        // ── Manage Stories (có phân trang + tìm kiếm) ─────────────────────
        public async Task<IActionResult> ManageStories(string? keyword, string? status,
                                                        int page = 1, int pageSize = 15)
        {
            var stories = (await _storyRepo.GetAllAsync()).AsEnumerable();

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
            ViewBag.BaseUrl = Url.Action("ManageStories")
                + BuildQueryExcludePage(new { keyword, status, pageSize });

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
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
            });
            TempData["Success"] = "Thêm tài khoản thành công!";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var currentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (id == currentId) { TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập!"; return RedirectToAction("ManageUsers"); }
            var user = _userRepo.GetById(id);
            if (user != null) _userRepo.Delete(user);
            TempData["Success"] = "Xóa tài khoản thành công!";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangeRole(int id, string role)
        {
            var currentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
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

        // Helper: build query string giữ lại filter, bỏ page
        private static string BuildQueryExcludePage(object? filters)
        {
            if (filters == null) return "";
            var parts = new List<string>();
            foreach (var prop in filters.GetType().GetProperties())
            {
                var val = prop.GetValue(filters);
                if (val != null && val.ToString() != "")
                    parts.Add($"{prop.Name}={Uri.EscapeDataString(val.ToString()!)}");
            }
            return parts.Any() ? "?" + string.Join("&", parts) : "";
        }
    }
}