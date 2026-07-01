using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class StoryController : Controller
    {
        private readonly IStoryService _storyService;
        private readonly IGenreService _genreService;
        private readonly ICommentService _commentService;
        private readonly IFavoriteService _favoriteService;
        private readonly IRatingService _ratingService;

        public StoryController(
            IStoryService storyService,
            IGenreService genreService,
            ICommentService commentService,
            IFavoriteService favoriteService,
            IRatingService ratingService)
        {
            _storyService = storyService;
            _genreService = genreService;
            _commentService = commentService;
            _favoriteService = favoriteService;
            _ratingService = ratingService;
        }

        private int? CurrentUserId =>
            User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                : null;

        // ── Index: trả về List<StoryDto> ─────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Index(string keyword, List<int> genreIds,
            string status, string sort, int page = 1, int pageSize = 12)
        {
            IEnumerable<StoryDto> stories = string.IsNullOrEmpty(keyword)
                ? await _storyService.GetAllStoriesAsync()
                : await _storyService.SearchAsync(keyword);

            if (genreIds?.Any() == true)
                stories = stories.Where(s => genreIds.All(id => s.Genres.Any(g => g.GenreId == id)));
            if (!string.IsNullOrEmpty(status))
                stories = stories.Where(s => s.Status == status);

            stories = sort switch
            {
                "updated" => stories.OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt),
                "chapter" => stories.OrderByDescending(s => s.ChapterCount),
                "follow" => stories.OrderByDescending(s => s.FavoriteCount),
                "rating" => stories.OrderByDescending(s => s.AvgRating),
                _ => stories.OrderByDescending(s => s.CreatedAt)
            };

            int total = stories.Count();

            ViewBag.Genres = (await _genreService.GetAllAsync())
                .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            ViewBag.GenreIds = genreIds ?? new List<int>();
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalStories = total;

            return View(stories.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        // ── Details: trả về StoryDetailDto ───────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _storyService.GetDetailAsync(id, CurrentUserId);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // ── Comment ───────────────────────────────────────────────────────
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int storyId, string content, int? chapterId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Bình luận không được để trống.";
                return chapterId.HasValue
                    ? RedirectToAction("ReadChapter", "Chapter", new { id = chapterId })
                    : RedirectToAction("Details", new { id = storyId });
            }

            await _commentService.AddAsync(storyId, CurrentUserId!.Value, content, chapterId);

            return chapterId.HasValue
                ? RedirectToAction("ReadChapter", "Chapter", new { id = chapterId })
                : RedirectToAction("Details", new { id = storyId });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int storyId, int? chapterId = null)
        {
            bool ok = await _commentService.DeleteAsync(commentId, CurrentUserId!.Value, User.IsInRole("Admin"));
            if (!ok) return Forbid();

            return chapterId.HasValue
                ? RedirectToAction("ReadChapter", "Chapter", new { id = chapterId })
                : RedirectToAction("Details", new { id = storyId });
        }

        // ── Favorite (AJAX) ───────────────────────────────────────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> ToggleFavorite(int storyId)
        {
            var result = await _favoriteService.ToggleAsync(CurrentUserId!.Value, storyId);
            return Json(result);
        }

        // ── Rating (AJAX) — 1..5 ─────────────────────────────────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> Rate(int storyId, int score)
        {
            if (score < 1 || score > 5) return BadRequest(new { error = "Điểm phải từ 1–5" });
            var result = await _ratingService.RateAsync(CurrentUserId!.Value, storyId, score);
            return Json(result);
        }

        // ── Create (Editor) ──────────────────────────────────────────────
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = (await _genreService.GetAllAsync()).ToList();
            return View(new StoryFormDto());
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryFormDto dto, IFormFile? coverImage)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = (await _genreService.GetAllAsync()).ToList();
                return View(dto);
            }

            using var stream = coverImage?.Length > 0 ? coverImage.OpenReadStream() : null;
            await _storyService.CreateAsync(dto, CurrentUserId!.Value, stream, coverImage?.FileName);

            TempData["Success"] = "Thêm truyện thành công!";
            return RedirectToAction("ManageStories", "Editor");
        }

        // ── Edit (Editor) ─────────────────────────────────────────────────
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(int id)
        {
            // GetFormDtoAsync không tự check quyền sở hữu → kiểm tra qua GetDetailAsync
            var detail = await _storyService.GetDetailAsync(id, CurrentUserId);
            if (detail == null) return NotFound();
            if (detail.CreatedBy != CurrentUserId) return Forbid();

            var dto = await _storyService.GetFormDtoAsync(id);
            if (dto == null) return NotFound();

            ViewBag.Genres = (await _genreService.GetAllAsync()).ToList();
            return View(dto);
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StoryFormDto dto, IFormFile? coverImage)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = (await _genreService.GetAllAsync()).ToList();
                return View(dto);
            }

            using var stream = coverImage?.Length > 0 ? coverImage.OpenReadStream() : null;
            bool ok = await _storyService.UpdateAsync(dto, CurrentUserId!.Value, stream, coverImage?.FileName);
            if (!ok) return Forbid();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("ManageStories", "Editor");
        }

        // ── Delete (Editor) ───────────────────────────────────────────────
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Delete(int id)
        {
            var detail = await _storyService.GetDetailAsync(id, null);
            if (detail == null) return NotFound();
            if (detail.CreatedBy != CurrentUserId) return Forbid();

            return View(detail); // StoryDetailDto kế thừa StoryDto, view nhận StoryDto vẫn hoạt động
        }

        [HttpPost, ActionName("Delete"), Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool ok = await _storyService.DeleteAsync(id, CurrentUserId!.Value);
            if (!ok) return Forbid();

            TempData["Success"] = "Xóa truyện thành công!";
            return RedirectToAction("ManageStories", "Editor");
        }
    }
}