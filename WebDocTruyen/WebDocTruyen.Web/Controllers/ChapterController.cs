using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class ChapterController : Controller
    {
        private readonly IChapterRepository _chapterRepo;
        private readonly IStoryRepository _storyRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IFavoriteRepository _favoriteRepo;
        private readonly IWebHostEnvironment _env;

        public ChapterController(IChapterRepository chapterRepo, IStoryRepository storyRepo,
            ICommentRepository commentRepo, IFavoriteRepository favoriteRepo, IWebHostEnvironment env)
        {
            _chapterRepo = chapterRepo;
            _storyRepo = storyRepo;
            _commentRepo = commentRepo;
            _favoriteRepo = favoriteRepo;
            _env = env;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private async Task<bool> OwnsStoryAsync(int storyId)
        {
            var story = await _storyRepo.GetByIdAsync(storyId);
            return story?.CreatedBy == CurrentUserId;
        }

        private string ImageFolder(int storyId, int chapterNumber) =>
            Path.Combine(_env.WebRootPath, "images", "stories", storyId.ToString(), chapterNumber.ToString());

        private async Task<string> SaveImageFileAsync(IFormFile file, int storyId, int chapterNumber)
        {
            var folder = ImageFolder(storyId, chapterNumber);
            Directory.CreateDirectory(folder);
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/images/stories/{storyId}/{chapterNumber}/{fileName}";
        }

        // ── Public: ListChapters — trả về StoryDetailDto ─────────────────
        [AllowAnonymous]
        public async Task<IActionResult> ListChapters(int id, int chapPage = 1, int chapSize = 20)
        {
            var story = await _chapterRepo.GetStoryWithChaptersAsync(id);
            if (story == null) return NotFound();

            var allChaps = story.Chapters.OrderBy(c => c.ChapterNumber).ToList();
            int total = allChaps.Count;
            int pages = Math.Max(1, (int)Math.Ceiling((double)total / chapSize));
            chapPage = Math.Clamp(chapPage, 1, pages);

            var pagedDtos = allChaps
                .Skip((chapPage - 1) * chapSize).Take(chapSize)
                .Select(ChapterMapper.ToSummary).ToList();

            ViewBag.ChapPage = chapPage;
            ViewBag.ChapPages = pages;
            ViewBag.ChapTotal = total;
            ViewBag.ChapSize = chapSize;
            ViewBag.PagedChapters = pagedDtos;
            ViewBag.StoryId = story.StoryId;
            ViewBag.StoryTitle = story.Title;
            ViewBag.StoryCover = story.CoverImage;
            ViewBag.StoryStatus = story.Status;
            ViewBag.StoryAuthor = story.Author;

            return View(pagedDtos);
        }

        // ── Public: ReadChapter — trả về ChapterReadDto ───────────────────
        [AllowAnonymous]
        public async Task<IActionResult> ReadChapter(int id)
        {
            var chapter = await _chapterRepo.GetByIdWithImagesAsync(id);
            if (chapter == null) return NotFound();

            var story = await _storyRepo.GetByIdAsync(chapter.StoryId);
            var allChapters = (await _chapterRepo.GetByStoryIdAsync(chapter.StoryId))
                              .OrderBy(c => c.ChapterNumber).ToList();

            // Ảnh: ưu tiên DB, fallback disk
            var imageEntities = chapter.ChapterImages.OrderBy(i => i.PageNumber).ToList();
            if (!imageEntities.Any())
            {
                var folder = ImageFolder(chapter.StoryId, chapter.ChapterNumber);
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (Directory.Exists(folder))
                {
                    int pn = 1;
                    foreach (var fp in Directory.GetFiles(folder)
                        .Where(f => allowed.Contains(Path.GetExtension(f).ToLower())).OrderBy(f => f))
                    {
                        imageEntities.Add(new ChapterImage
                        {
                            ImageId = 0,
                            ChapterId = chapter.ChapterId,
                            PageNumber = pn++,
                            ImageUrl = $"/images/stories/{chapter.StoryId}/{chapter.ChapterNumber}/{Path.GetFileName(fp)}"
                        });
                    }
                }
            }

            var allDtos = allChapters.Select(ChapterMapper.ToSummary).ToList();
            int idx = allChapters.FindIndex(c => c.ChapterId == id);
            var prevDto = idx > 0 ? ChapterMapper.ToSummary(allChapters[idx - 1]) : null;
            var nextDto = idx < allChapters.Count - 1 ? ChapterMapper.ToSummary(allChapters[idx + 1]) : null;

            var commentEntities = await _commentRepo.GetByChapterIdAsync(id);
            var comments = commentEntities.Select(CommentMapper.ToDto).ToList();

            bool isFav = false;
            int favCount = await _favoriteRepo.CountAsync(chapter.StoryId);
            var uidClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (uidClaim != null)
                isFav = await _favoriteRepo.IsFavoritedAsync(int.Parse(uidClaim.Value), chapter.StoryId);

            var dto = ChapterMapper.ToReadDto(
                chapter, story,
                imageEntities.Select(ChapterMapper.ToImageDto).ToList(),
                allDtos, prevDto, nextDto, comments, isFav, favCount);

            return View(dto);
        }

        // ── Editor: Chapter CRUD ──────────────────────────────────────────

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> AddChapter(int storyId)
        {
            if (!await OwnsStoryAsync(storyId)) return Forbid();
            ViewBag.Story = await _storyRepo.GetByIdAsync(storyId);
            return View(new Chapter());
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChapter(int storyId, Chapter chapter)
        {
            if (!await OwnsStoryAsync(storyId)) return Forbid();
            ModelState.Remove("Story"); ModelState.Remove("ChapterImages"); ModelState.Remove("Comments");
            if (!ModelState.IsValid) { ViewBag.Story = await _storyRepo.GetByIdAsync(storyId); return View(chapter); }
            if (await _chapterRepo.GetByStoryAndNumberAsync(storyId, chapter.ChapterNumber) != null)
            {
                ModelState.AddModelError("", $"Chapter {chapter.ChapterNumber} đã tồn tại.");
                ViewBag.Story = await _storyRepo.GetByIdAsync(storyId); return View(chapter);
            }
            chapter.StoryId = storyId;
            await _chapterRepo.AddAsync(chapter);
            Directory.CreateDirectory(ImageFolder(storyId, chapter.ChapterNumber));
            TempData["Success"] = $"Thêm Chapter {chapter.ChapterNumber} thành công!";
            return RedirectToAction(nameof(ManageImages), new { chapterId = chapter.ChapterId });
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> EditChapter(int id)
        {
            var chapter = await _chapterRepo.GetByIdAsync(id);
            if (chapter == null) return NotFound();
            if (!await OwnsStoryAsync(chapter.StoryId)) return Forbid();
            ViewBag.Story = await _storyRepo.GetByIdAsync(chapter.StoryId);
            return View(chapter);
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(Chapter chapter)
        {
            var old = await _chapterRepo.GetByIdAsync(chapter.ChapterId);
            if (old == null) return NotFound();
            if (!await OwnsStoryAsync(old.StoryId)) return Forbid();
            ModelState.Remove("Story"); ModelState.Remove("ChapterImages"); ModelState.Remove("Comments");
            if (!ModelState.IsValid) { ViewBag.Story = await _storyRepo.GetByIdAsync(chapter.StoryId); return View(chapter); }
            var dup = await _chapterRepo.GetByStoryAndNumberAsync(chapter.StoryId, chapter.ChapterNumber);
            if (dup != null && dup.ChapterId != chapter.ChapterId)
            {
                ModelState.AddModelError("", $"Chapter {chapter.ChapterNumber} đã tồn tại.");
                ViewBag.Story = await _storyRepo.GetByIdAsync(chapter.StoryId); return View(chapter);
            }
            if (old.ChapterNumber != chapter.ChapterNumber)
            {
                var oldDir = ImageFolder(old.StoryId, old.ChapterNumber);
                var newDir = ImageFolder(chapter.StoryId, chapter.ChapterNumber);
                if (Directory.Exists(oldDir)) Directory.Move(oldDir, newDir);
                else Directory.CreateDirectory(newDir);
                var imgs = await _chapterRepo.GetImagesByChapterIdAsync(chapter.ChapterId);
                foreach (var img in imgs) { img.ImageUrl = img.ImageUrl.Replace($"/{old.ChapterNumber}/", $"/{chapter.ChapterNumber}/"); await _chapterRepo.UpdateImageAsync(img); }
            }
            old.Title = chapter.Title; old.ChapterNumber = chapter.ChapterNumber;
            await _chapterRepo.UpdateAsync(old);
            TempData["Success"] = "Cập nhật chapter thành công!";
            return RedirectToAction(nameof(ListChapters), new { id = chapter.StoryId });
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var chapter = await _chapterRepo.GetByIdWithImagesAsync(id);
            if (chapter == null) return NotFound();
            if (!await OwnsStoryAsync(chapter.StoryId)) return Forbid();
            return View(chapter);
        }

        [HttpPost, ActionName("DeleteChapter"), Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChapterConfirmed(int id)
        {
            var chapter = await _chapterRepo.GetByIdAsync(id);
            if (chapter == null) return NotFound();
            if (!await OwnsStoryAsync(chapter.StoryId)) return Forbid();
            var dir = ImageFolder(chapter.StoryId, chapter.ChapterNumber);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            int storyId = chapter.StoryId;
            await _chapterRepo.DeleteAsync(id);
            TempData["Success"] = "Xóa chapter thành công!";
            return RedirectToAction(nameof(ListChapters), new { id = storyId });
        }

        // ── Editor: Image CRUD ────────────────────────────────────────────

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> ManageImages(int chapterId)
        {
            var chapter = await _chapterRepo.GetByIdWithImagesAsync(chapterId);
            if (chapter == null) return NotFound();
            if (!await OwnsStoryAsync(chapter.StoryId)) return Forbid();
            var story = await _storyRepo.GetByIdAsync(chapter.StoryId);
            var dto = ChapterMapper.ToManageDto(chapter, story);
            return View(dto);
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int chapterId, List<IFormFile> imageFiles)
        {
            var chapter = await _chapterRepo.GetByIdAsync(chapterId);
            if (chapter == null) return NotFound();
            if (!await OwnsStoryAsync(chapter.StoryId)) return Forbid();
            if (imageFiles == null || !imageFiles.Any()) { TempData["Error"] = "Chọn ít nhất 1 ảnh."; return RedirectToAction(nameof(ManageImages), new { chapterId }); }
            var existing = await _chapterRepo.GetImagesByChapterIdAsync(chapterId);
            int nextPage = existing.Any() ? existing.Max(i => i.PageNumber) + 1 : 1;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var newImgs = new List<ChapterImage>();
            foreach (var file in imageFiles.OrderBy(f => f.FileName))
            {
                if (!allowed.Contains(Path.GetExtension(file.FileName).ToLower()) || file.Length > 10 * 1024 * 1024) continue;
                newImgs.Add(new ChapterImage { ChapterId = chapterId, ImageUrl = await SaveImageFileAsync(file, chapter.StoryId, chapter.ChapterNumber), PageNumber = nextPage++ });
            }
            if (newImgs.Any()) { await _chapterRepo.AddImagesAsync(newImgs); TempData["Success"] = $"Đã upload {newImgs.Count} ảnh!"; }
            else TempData["Error"] = "Không có ảnh hợp lệ nào.";
            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> EditImage(int imageId)
        {
            var image = await _chapterRepo.GetImageByIdAsync(imageId);
            if (image == null) return NotFound();
            var chapter = await _chapterRepo.GetByIdAsync(image.ChapterId);
            if (!await OwnsStoryAsync(chapter!.StoryId)) return Forbid();
            ViewBag.Chapter = ChapterMapper.ToSummary(chapter);
            return View(ChapterMapper.ToImageDto(image));
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(int imageId, int pageNumber, IFormFile? newImageFile)
        {
            var image = await _chapterRepo.GetImageByIdAsync(imageId);
            if (image == null) return NotFound();
            var chapter = await _chapterRepo.GetByIdAsync(image.ChapterId);
            if (!await OwnsStoryAsync(chapter!.StoryId)) return Forbid();
            if (newImageFile?.Length > 0)
            {
                var oldFile = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                image.ImageUrl = await SaveImageFileAsync(newImageFile, chapter.StoryId, chapter.ChapterNumber);
            }
            image.PageNumber = pageNumber;
            await _chapterRepo.UpdateImageAsync(image);
            TempData["Success"] = $"Đã cập nhật trang {pageNumber}!";
            return RedirectToAction(nameof(ManageImages), new { chapterId = image.ChapterId });
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _chapterRepo.GetImageByIdAsync(imageId);
            if (image == null) return NotFound();
            var chapter = await _chapterRepo.GetByIdAsync(image.ChapterId);
            if (!await OwnsStoryAsync(chapter!.StoryId)) return Forbid();
            var filePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            int chapterId = image.ChapterId;
            await _chapterRepo.DeleteImageAsync(imageId);
            var remaining = (await _chapterRepo.GetImagesByChapterIdAsync(chapterId)).ToList();
            for (int i = 0; i < remaining.Count; i++) { remaining[i].PageNumber = i + 1; await _chapterRepo.UpdateImageAsync(remaining[i]); }
            TempData["Success"] = "Đã xóa ảnh!";
            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllImages(int chapterId)
        {
            var chapter = await _chapterRepo.GetByIdAsync(chapterId);
            if (chapter == null) return NotFound();
            if (!await OwnsStoryAsync(chapter.StoryId)) return Forbid();
            var dir = ImageFolder(chapter.StoryId, chapter.ChapterNumber);
            if (Directory.Exists(dir)) foreach (var f in Directory.GetFiles(dir)) System.IO.File.Delete(f);
            await _chapterRepo.DeleteAllImagesAsync(chapterId);
            TempData["Success"] = "Đã xóa toàn bộ ảnh!";
            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [HttpPost, Authorize(Roles = "Editor")]
        public async Task<IActionResult> ReorderImages([FromBody] ReorderRequest req)
        {
            if (req?.ImageIds == null || !req.ImageIds.Any()) return BadRequest(new { success = false });
            var first = await _chapterRepo.GetImageByIdAsync(req.ImageIds[0]);
            if (first == null) return NotFound();
            var chapter = await _chapterRepo.GetByIdAsync(first.ChapterId);
            if (!await OwnsStoryAsync(chapter!.StoryId)) return Forbid();
            await _chapterRepo.ReorderImagesAsync(first.ChapterId, req.ImageIds);
            return Ok(new { success = true });
        }
    }

    public class ReorderRequest { public List<int> ImageIds { get; set; } = new(); }
}