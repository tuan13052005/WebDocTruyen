using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class ChapterController : Controller
    {
        private readonly IChapterService _chapterService;

        public ChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private int? CurrentUserIdOrNull =>
            User.Identity?.IsAuthenticated == true ? CurrentUserId : null;

        // ── Public: ListChapters ──────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> ListChapters(int id, int chapPage = 1, int chapSize = 20)
        {
            var (items, total, story) = await _chapterService.GetListAsync(id, chapPage, chapSize);
            if (story == null) return NotFound();

            int pages = Math.Max(1, (int)Math.Ceiling((double)total / chapSize));
            chapPage = Math.Clamp(chapPage, 1, pages);

            ViewBag.ChapPage = chapPage;
            ViewBag.ChapPages = pages;
            ViewBag.ChapTotal = total;
            ViewBag.ChapSize = chapSize;
            ViewBag.PagedChapters = items;
            ViewBag.StoryId = story.Id;
            ViewBag.StoryTitle = story.Title;
            ViewBag.StoryCover = story.CoverImage;
            ViewBag.StoryStatus = story.Status;
            ViewBag.StoryAuthor = story.Author;

            return View(items);
        }

        // ── Public: ReadChapter ────────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> ReadChapter(int id)
        {
            var dto = await _chapterService.GetReadDtoAsync(id, CurrentUserIdOrNull);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // ── Editor: Chapter CRUD ───────────────────────────────────────────

        [Authorize(Roles = "Editor")]
        public IActionResult AddChapter(int storyId)
        {
            return View(new ChapterFormDto { StoryId = storyId });
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChapter(int storyId, ChapterFormDto dto, List<IFormFile> imageFiles)
        {
            dto.StoryId = storyId;
            if (!ModelState.IsValid) return View(dto);

            try
            {
                int chapterId = await _chapterService.CreateAsync(storyId, dto, CurrentUserId);

                if (imageFiles != null && imageFiles.Any())
                {
                    var files = imageFiles.OrderBy(f => f.FileName)
                        .Select(f => (f.OpenReadStream(), f.FileName, f.Length))
                        .ToList();

                    try
                    {
                        int count = await _chapterService.UploadImagesAsync(chapterId, CurrentUserId, files);
                        TempData["Success"] = $"Thêm Chapter {dto.ChapterNumber} thành công! Đã upload {count} ảnh.";
                    }
                    finally
                    {
                        foreach (var (stream, _, _) in files) stream.Dispose();
                    }
                }
                else
                {
                    TempData["Success"] = $"Thêm Chapter {dto.ChapterNumber} thành công!";
                }

                return RedirectToAction(nameof(ManageImages), new { chapterId });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> EditChapter(int id)
        {
            var dto = await _chapterService.GetFormDtoAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(ChapterFormDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                bool ok = await _chapterService.UpdateAsync(dto, CurrentUserId);
                if (!ok) return Forbid();

                TempData["Success"] = "Cập nhật chapter thành công!";
                return RedirectToAction(nameof(ListChapters), new { id = dto.StoryId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var dto = await _chapterService.GetDeleteDtoAsync(id, CurrentUserId);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost, ActionName("DeleteChapter"), Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChapterConfirmed(int id)
        {
            var dto = await _chapterService.GetDeleteDtoAsync(id, CurrentUserId);
            if (dto == null) return NotFound();

            bool ok = await _chapterService.DeleteAsync(id, CurrentUserId);
            if (!ok) return Forbid();

            TempData["Success"] = "Xóa chapter thành công!";
            return RedirectToAction(nameof(ListChapters), new { id = dto.StoryId });
        }

        // ── Editor: Image CRUD ────────────────────────────────────────────

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> ManageImages(int chapterId)
        {
            var dto = await _chapterService.GetManageDtoAsync(chapterId, CurrentUserId);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int chapterId, List<IFormFile> imageFiles)
        {
            if (imageFiles == null || !imageFiles.Any())
            {
                TempData["Error"] = "Chọn ít nhất 1 ảnh.";
                return RedirectToAction(nameof(ManageImages), new { chapterId });
            }

            var files = imageFiles.OrderBy(f => f.FileName)
                .Select(f => (f.OpenReadStream(), f.FileName, f.Length))
                .ToList();

            try
            {
                int count = await _chapterService.UploadImagesAsync(chapterId, CurrentUserId, files);
                if (count > 0) TempData["Success"] = $"Đã upload {count} ảnh!";
                else TempData["Error"] = "Không có ảnh hợp lệ nào.";
            }
            finally
            {
                foreach (var (stream, _, _) in files) stream.Dispose();
            }

            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> EditImage(int imageId)
        {
            var image = await _chapterService.GetImageDtoAsync(imageId, CurrentUserId);
            if (image == null) return NotFound();
            return View(image);
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(int imageId, int chapterId, int pageNumber, IFormFile? newImageFile)
        {
            (Stream Content, string FileName)? newImage = newImageFile?.Length > 0
                ? (newImageFile.OpenReadStream(), newImageFile.FileName)
                : null;

            try
            {
                bool ok = await _chapterService.UpdateImageAsync(imageId, CurrentUserId, pageNumber, newImage);
                if (!ok) return Forbid();
            }
            finally
            {
                newImage?.Content.Dispose();
            }

            TempData["Success"] = $"Đã cập nhật trang {pageNumber}!";
            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int chapterId)
        {
            bool ok = await _chapterService.DeleteImageAsync(imageId, CurrentUserId);
            if (!ok) return Forbid();

            TempData["Success"] = "Đã xóa ảnh!";
            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [HttpPost, Authorize(Roles = "Editor"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllImages(int chapterId)
        {
            bool ok = await _chapterService.DeleteAllImagesAsync(chapterId, CurrentUserId);
            if (!ok) return Forbid();

            TempData["Success"] = "Đã xóa toàn bộ ảnh!";
            return RedirectToAction(nameof(ManageImages), new { chapterId });
        }

        [HttpPost, Authorize(Roles = "Editor")]
        public async Task<IActionResult> ReorderImages([FromBody] ReorderRequest req)
        {
            if (req?.ImageIds == null || !req.ImageIds.Any()) return BadRequest(new { success = false });

            bool ok = await _chapterService.ReorderImagesAsync(req.ImageIds[0], CurrentUserId, req.ImageIds);
            if (!ok) return Forbid();

            return Ok(new { success = true });
        }
    }

    public class ReorderRequest { public List<int> ImageIds { get; set; } = new(); }
}