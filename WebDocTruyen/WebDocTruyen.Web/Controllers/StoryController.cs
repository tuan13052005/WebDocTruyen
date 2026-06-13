using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Application.Services;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class StoryController : Controller
    {
        private readonly IStoryRepository _storyRepo;
        private readonly IGenreRepository _genreRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IFavoriteRepository _favoriteRepo;
        private readonly IRatingRepository _ratingRepo;
        private readonly StoryService _storyService;

        public StoryController(IStoryRepository storyRepo, IGenreRepository genreRepo,
            ICommentRepository commentRepo, IFavoriteRepository favoriteRepo,
            IRatingRepository ratingRepo, StoryService storyService)
        {
            _storyRepo = storyRepo;
            _genreRepo = genreRepo;
            _commentRepo = commentRepo;
            _favoriteRepo = favoriteRepo;
            _ratingRepo = ratingRepo;
            _storyService = storyService;
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
                stories = stories.Where(s => s.Genres.Any(g => genreIds.Contains(g.GenreId)));
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
            ViewBag.Genres = await _genreRepo.GetAllGenreAsync();
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
            var story = await _storyRepo.GetByIdAsync(id);
            if (story == null) return NotFound();

            var commentEntities = await _commentRepo.GetByStoryIdAsync(id);
            var comments = commentEntities.Select(CommentMapper.ToDto).ToList();
            var avgRating = await _ratingRepo.GetAverageAsync(id);
            var ratingCount = await _ratingRepo.CountAsync(id);
            var favCount = await _favoriteRepo.CountAsync(id);
            bool isFav = false;
            int? myRating = null;

            if (CurrentUserId.HasValue)
            {
                isFav = await _favoriteRepo.IsFavoritedAsync(CurrentUserId.Value, id);
                var myR = await _ratingRepo.GetByUserAndStoryAsync(CurrentUserId.Value, id);
                myRating = myR?.Score;
            }

            var dto = StoryMapper.ToDetailDto(story, comments, avgRating,
                ratingCount, favCount, isFav, myRating, CurrentUserId);

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
            await _commentRepo.AddAsync(new Comment
            {
                StoryId = storyId,
                ChapterId = chapterId,
                UserId = CurrentUserId!.Value,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            });
            return chapterId.HasValue
                ? RedirectToAction("ReadChapter", "Chapter", new { id = chapterId })
                : RedirectToAction("Details", new { id = storyId });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int storyId, int? chapterId = null)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null) return NotFound();
            if (comment.UserId != CurrentUserId && !User.IsInRole("Admin")) return Forbid();
            await _commentRepo.DeleteAsync(commentId);
            return chapterId.HasValue
                ? RedirectToAction("ReadChapter", "Chapter", new { id = chapterId })
                : RedirectToAction("Details", new { id = storyId });
        }

        // ── Favorite (AJAX) ───────────────────────────────────────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> ToggleFavorite(int storyId)
        {
            int uid = CurrentUserId!.Value;
            var fav = await _favoriteRepo.GetAsync(uid, storyId);
            if (fav != null) await _favoriteRepo.DeleteAsync(fav.FavoriteId);
            else await _favoriteRepo.AddAsync(new Favorite { UserId = uid, StoryId = storyId });
            int count = await _favoriteRepo.CountAsync(storyId);
            bool now = fav == null;
            return Json(new { favorited = now, count });
        }

        // ── Rating (AJAX) — FIX: validate 1-10 thay vì 1-5 ──────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> Rate(int storyId, int score)
        {
            if (score < 1 || score > 5) return BadRequest(new { error = "Điểm phải từ 1–5" });
            int uid = CurrentUserId!.Value;
            var existing = await _ratingRepo.GetByUserAndStoryAsync(uid, storyId);
            if (existing != null) { existing.Score = score; await _ratingRepo.UpdateAsync(existing); }
            else await _ratingRepo.AddAsync(new Rating { UserId = uid, StoryId = storyId, Score = score, CreatedAt = DateTime.Now });
            double avg = await _ratingRepo.GetAverageAsync(storyId);
            int cnt = await _ratingRepo.CountAsync(storyId);
            return Json(new { avg = Math.Round(avg, 1), count = cnt, myScore = score });
        }

        // ── Create / Edit / Delete (Editor) ──────────────────────────────
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = await _genreRepo.GetAllGenreAsync();
            return View(new StoryFormDto());
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryFormDto dto, IFormFile? coverImage)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = await _genreRepo.GetAllGenreAsync();
                return View(dto);
            }
            var story = new Story
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                Status = dto.Status,
                CreatedAt = DateTime.Now,
                CreatedBy = CurrentUserId!.Value,
                StoryGenres = new List<StoryGenre>()
            };
            await _storyRepo.AddAsync(story);

            if (coverImage?.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stories", story.StoryId.ToString());
                Directory.CreateDirectory(folder);
                var fn = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                using var s = new FileStream(Path.Combine(folder, fn), FileMode.Create);
                await coverImage.CopyToAsync(s);
                story.CoverImage = $"/images/stories/{story.StoryId}/{fn}";
            }
            story.StoryGenres = dto.SelectedGenreIds
                .Select(g => new StoryGenre { GenreId = g, StoryId = story.StoryId }).ToList();
            await _storyRepo.UpdateAsync(story);
            TempData["Success"] = "Thêm truyện thành công!";
            return RedirectToAction("ManageStories", "Editor");
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(int id)
        {
            var story = await _storyRepo.GetByIdAsync(id);
            if (story == null) return NotFound();
            if (story.CreatedBy != CurrentUserId) return Forbid();
            ViewBag.Genres = await _genreRepo.GetAllGenreAsync();
            return View(StoryMapper.ToFormDto(story));
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StoryFormDto dto, IFormFile? coverImage)
        {
            var existing = await _storyRepo.GetByIdAsync(dto.StoryId);
            if (existing == null) return NotFound();
            if (existing.CreatedBy != CurrentUserId) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Genres = await _genreRepo.GetAllGenreAsync();
                return View(dto);
            }

            existing.Title = dto.Title;
            existing.Author = dto.Author;
            existing.Description = dto.Description;
            existing.Status = dto.Status;
            existing.UpdatedAt = DateTime.Now;

            if (coverImage?.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stories", existing.StoryId.ToString());
                Directory.CreateDirectory(folder);
                var fn = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                using var s = new FileStream(Path.Combine(folder, fn), FileMode.Create);
                await coverImage.CopyToAsync(s);
                existing.CoverImage = $"/images/stories/{existing.StoryId}/{fn}";
            }
            existing.StoryGenres = dto.SelectedGenreIds
                .Select(g => new StoryGenre { GenreId = g, StoryId = existing.StoryId }).ToList();
            await _storyRepo.UpdateAsync(existing);
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("ManageStories", "Editor");
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Delete(int id)
        {
            var story = await _storyRepo.GetByIdAsync(id);
            if (story == null) return NotFound();
            if (story.CreatedBy != CurrentUserId) return Forbid();
            return View(StoryMapper.ToDto(story));
        }

        [HttpPost, ActionName("Delete"), Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var story = await _storyRepo.GetByIdAsync(id);
            if (story == null) return NotFound();
            if (story.CreatedBy != CurrentUserId) return Forbid();
            await _storyRepo.DeleteAsync(id);
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stories", id.ToString());
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            TempData["Success"] = "Xóa truyện thành công!";
            return RedirectToAction("ManageStories", "Editor");
        }
    }
}