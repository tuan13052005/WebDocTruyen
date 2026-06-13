using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Application.Services;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    [Authorize(Roles = "Editor")]
    public class EditorController : Controller
    {
        private readonly StoryService _storyService;
        private readonly IStoryRepository _storyRepo;

        public EditorController(StoryService storyService, IStoryRepository storyRepo)
        {
            _storyService = storyService;
            _storyRepo = storyRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> Dashboard()
        {
            var all = await _storyRepo.GetAllAsync();
            var myStories = all.Where(s => s.CreatedBy == CurrentUserId)
                                .Select(StoryMapper.ToDto)
                                .ToList();

            ViewBag.TotalStories = myStories.Count;
            ViewBag.TotalOngoingStories = myStories.Count(s => s.Status == "ongoing");
            ViewBag.TotalCompletedStories = myStories.Count(s => s.Status == "completed");
            ViewBag.LatestStory = myStories.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
            ViewBag.RecentStories = myStories.OrderByDescending(s => s.CreatedAt).Take(5).ToList();
            return View();
        }

        // ManageStories: chỉ hiển thị các truyện do editor hiện tại tạo
        public async Task<IActionResult> ManageStories(string? keyword, string? status, int page = 1, int pageSize = 10)
        {
            var all = await _storyRepo.GetAllAsync();
            var stories = all.Where(s => s.CreatedBy == CurrentUserId)
                              .Select(StoryMapper.ToDto)
                              .AsEnumerable();

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