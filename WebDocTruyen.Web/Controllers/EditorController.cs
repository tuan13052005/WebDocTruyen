using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebDocTruyen.Application.Services;

namespace WebDocTruyen.Web.Controllers
{
    [Authorize(Roles = "Editor")]
    public class EditorController : Controller
    {
        private readonly StoryService _storyService;
        public EditorController(StoryService storyService) { _storyService = storyService; }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> Dashboard()
        {
            var all = await _storyService.GetAllStoriesAsync();
            var stories = all.Where(s => s.ChapterCount >= 0).ToList(); // tất cả của mình — xem bên dưới
            // Note: StoryDto không có CreatedBy, cần query riêng qua storyRepo nếu cần lọc theo editor
            // Tạm thời giữ ViewBag thống kê cơ bản
            ViewBag.TotalStories = stories.Count;
            ViewBag.TotalOngoingStories = stories.Count(s => s.Status == "ongoing");
            ViewBag.TotalCompletedStories = stories.Count(s => s.Status == "completed");
            ViewBag.LatestStory = stories.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
            ViewBag.RecentStories = stories.OrderByDescending(s => s.CreatedAt).Take(5).ToList();
            return View();
        }

        public async Task<IActionResult> ManageStories(string? keyword, string? status, int page = 1, int pageSize = 10)
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