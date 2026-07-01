// WebDocTruyen.Application/Services/ChapterService.cs
using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Storage;

namespace WebDocTruyen.Application.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepo;
        private readonly IStoryRepository _storyRepo;
        private readonly ICommentService _commentService;
        private readonly IFavoriteService _favoriteService;
        private readonly ISupabaseStorageService _storage;

        private static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageSize = 10 * 1024 * 1024; // 10 MB

        public ChapterService(
            IChapterRepository chapterRepo,
            IStoryRepository storyRepo,
            ICommentService commentService,
            IFavoriteService favoriteService,
            ISupabaseStorageService storage)
        {
            _chapterRepo = chapterRepo;
            _storyRepo = storyRepo;
            _commentService = commentService;
            _favoriteService = favoriteService;
            _storage = storage;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private async Task<bool> OwnsStoryAsync(int storyId, int currentUserId)
        {
            var story = await _storyRepo.GetByIdAsync(storyId);
            return story?.CreatedBy == currentUserId;
        }

        private async Task<string> SaveImageFileAsync(
            Stream content, string fileName, int storyId, int chapterId)
        {
            var ext = Path.GetExtension(fileName);
            var path = $"stories/{storyId}/chapters/{chapterId}/{Guid.NewGuid()}{ext}";
            var contentType = _storage.GetContentType(fileName);
            return await _storage.UploadAsync(content, path, contentType);
        }

        // ── Danh sách chapter ─────────────────────────────────────────────

        public async Task<(List<ChapterSummaryDto> Items, int Total, StoryDto? Story)> GetListAsync(
            int storyId, int page, int pageSize)
        {
            var story = await _chapterRepo.GetStoryWithChaptersAsync(storyId);
            if (story == null) return (new(), 0, null);

            var all = story.Chapters.OrderBy(c => c.ChapterNumber).ToList();
            int total = all.Count;
            int pages = Math.Max(1, (int)Math.Ceiling((double)total / pageSize));
            page = Math.Clamp(page, 1, pages);

            var items = all.Skip((page - 1) * pageSize).Take(pageSize)
                           .Select(ChapterMapper.ToSummary).ToList();

            return (items, total, StoryMapper.ToDto(story));
        }

        // ── Trang đọc chapter ─────────────────────────────────────────────

        public async Task<ChapterReadDto?> GetReadDtoAsync(int chapterId, int? currentUserId)
        {
            var chapter = await _chapterRepo.GetByIdWithImagesAsync(chapterId);
            if (chapter == null) return null;

            var story = await _storyRepo.GetByIdAsync(chapter.StoryId);
            var allChapters = (await _chapterRepo.GetByStoryIdAsync(chapter.StoryId))
                              .OrderBy(c => c.ChapterNumber).ToList();

            // Ảnh lấy từ DB (đã là URL Supabase)
            var imageEntities = chapter.ChapterImages.OrderBy(i => i.PageNumber).ToList();

            var allDtos = allChapters.Select(ChapterMapper.ToSummary).ToList();
            int idx = allChapters.FindIndex(c => c.ChapterId == chapterId);
            var prevDto = idx > 0 ? ChapterMapper.ToSummary(allChapters[idx - 1]) : null;
            var nextDto = idx < allChapters.Count - 1 ? ChapterMapper.ToSummary(allChapters[idx + 1]) : null;

            var comments = await _commentService.GetByChapterIdAsync(chapterId);

            int favCount = await _favoriteService.CountAsync(chapter.StoryId);
            bool isFav = currentUserId.HasValue
                && await _favoriteService.IsFavoritedAsync(currentUserId.Value, chapter.StoryId);

            if (isFav && currentUserId.HasValue)
                await _favoriteService.MarkReadingAsync(currentUserId.Value, chapter.StoryId, chapterId);

            return ChapterMapper.ToReadDto(
                chapter, story,
                imageEntities.Select(ChapterMapper.ToImageDto).ToList(),
                allDtos, prevDto, nextDto, comments, isFav, favCount);
        }

        // ── Editor: CRUD chapter ──────────────────────────────────────────

        public async Task<ChapterFormDto?> GetFormDtoAsync(int chapterId)
        {
            var chapter = await _chapterRepo.GetByIdAsync(chapterId);
            if (chapter == null) return null;
            var story = await _storyRepo.GetByIdAsync(chapter.StoryId);
            return ChapterMapper.ToFormDto(chapter, story);
        }

        public async Task<int> CreateAsync(int storyId, ChapterFormDto dto, int currentUserId)
        {
            if (!await OwnsStoryAsync(storyId, currentUserId))
                throw new UnauthorizedAccessException();

            if (await _chapterRepo.GetByStoryAndNumberAsync(storyId, dto.ChapterNumber) != null)
                throw new InvalidOperationException($"Chapter {dto.ChapterNumber} đã tồn tại.");

            var chapter = new Chapter
            {
                StoryId = storyId,
                ChapterNumber = dto.ChapterNumber,
                Title = dto.Title
            };
            await _chapterRepo.AddAsync(chapter);
            return chapter.ChapterId;
        }

        public async Task<bool> UpdateAsync(ChapterFormDto dto, int currentUserId)
        {
            var old = await _chapterRepo.GetByIdAsync(dto.ChapterId);
            if (old == null) return false;
            if (!await OwnsStoryAsync(old.StoryId, currentUserId)) return false;

            var dup = await _chapterRepo.GetByStoryAndNumberAsync(old.StoryId, dto.ChapterNumber);
            if (dup != null && dup.ChapterId != dto.ChapterId)
                throw new InvalidOperationException($"Chapter {dto.ChapterNumber} đã tồn tại.");

            // Không cần move ảnh nữa vì path dùng ChapterId (cố định), không dùng ChapterNumber
            old.Title = dto.Title;
            old.ChapterNumber = dto.ChapterNumber;
            await _chapterRepo.UpdateAsync(old);
            return true;
        }

        public async Task<ChapterDeleteDto?> GetDeleteDtoAsync(int chapterId, int currentUserId)
        {
            var chapter = await _chapterRepo.GetByIdWithImagesAsync(chapterId);
            if (chapter == null) return null;
            if (!await OwnsStoryAsync(chapter.StoryId, currentUserId)) return null;
            return ChapterMapper.ToDeleteDto(chapter);
        }

        public async Task<bool> DeleteAsync(int chapterId, int currentUserId)
        {
            var chapter = await _chapterRepo.GetByIdAsync(chapterId);
            if (chapter == null) return false;
            if (!await OwnsStoryAsync(chapter.StoryId, currentUserId)) return false;

            // Xóa toàn bộ ảnh trên Supabase của chapter này
            await _storage.DeleteFolderAsync($"stories/{chapter.StoryId}/chapters/{chapterId}");

            await _chapterRepo.DeleteAsync(chapterId);
            return true;
        }

        // ── Editor: CRUD ảnh ──────────────────────────────────────────────

        public async Task<ChapterManageDto?> GetManageDtoAsync(int chapterId, int currentUserId)
        {
            var chapter = await _chapterRepo.GetByIdWithImagesAsync(chapterId);
            if (chapter == null) return null;
            if (!await OwnsStoryAsync(chapter.StoryId, currentUserId)) return null;
            var story = await _storyRepo.GetByIdAsync(chapter.StoryId);
            return ChapterMapper.ToManageDto(chapter, story);
        }

        public async Task<int> UploadImagesAsync(int chapterId, int currentUserId,
            IEnumerable<(Stream Content, string FileName, long Length)> files)
        {
            var chapter = await _chapterRepo.GetByIdAsync(chapterId);
            if (chapter == null) throw new KeyNotFoundException();
            if (!await OwnsStoryAsync(chapter.StoryId, currentUserId))
                throw new UnauthorizedAccessException();

            var existing = await _chapterRepo.GetImagesByChapterIdAsync(chapterId);
            int nextPage = existing.Any() ? existing.Max(i => i.PageNumber) + 1 : 1;
            var newImgs = new List<ChapterImage>();

            foreach (var (content, fileName, length) in files)
            {
                if (!AllowedExt.Contains(Path.GetExtension(fileName).ToLower())) continue;
                if (length > MaxImageSize) continue;

                var url = await SaveImageFileAsync(content, fileName, chapter.StoryId, chapterId);
                newImgs.Add(new ChapterImage { ChapterId = chapterId, ImageUrl = url, PageNumber = nextPage++ });
            }

            if (newImgs.Any()) await _chapterRepo.AddImagesAsync(newImgs);
            return newImgs.Count;
        }

        public async Task<bool> UpdateImageAsync(int imageId, int currentUserId, int pageNumber,
            (Stream Content, string FileName)? newImage)
        {
            var image = await _chapterRepo.GetImageByIdAsync(imageId);
            if (image == null) return false;
            var chapter = await _chapterRepo.GetByIdAsync(image.ChapterId);
            if (chapter == null || !await OwnsStoryAsync(chapter.StoryId, currentUserId)) return false;

            if (newImage.HasValue)
            {
                var oldPath = _storage.GetPathFromPublicUrl(image.ImageUrl);
                if (oldPath != null) await _storage.DeleteAsync(oldPath);

                image.ImageUrl = await SaveImageFileAsync(
                    newImage.Value.Content, newImage.Value.FileName,
                    chapter.StoryId, chapter.ChapterId);
            }

            image.PageNumber = pageNumber;
            await _chapterRepo.UpdateImageAsync(image);
            return true;
        }

        public async Task<bool> DeleteImageAsync(int imageId, int currentUserId)
        {
            var image = await _chapterRepo.GetImageByIdAsync(imageId);
            if (image == null) return false;
            var chapter = await _chapterRepo.GetByIdAsync(image.ChapterId);
            if (chapter == null || !await OwnsStoryAsync(chapter.StoryId, currentUserId)) return false;

            var path = _storage.GetPathFromPublicUrl(image.ImageUrl);
            if (path != null) await _storage.DeleteAsync(path);

            int chapterId = image.ChapterId;
            await _chapterRepo.DeleteImageAsync(imageId);

            // Cập nhật lại PageNumber liên tục
            var remaining = (await _chapterRepo.GetImagesByChapterIdAsync(chapterId)).ToList();
            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].PageNumber = i + 1;
                await _chapterRepo.UpdateImageAsync(remaining[i]);
            }
            return true;
        }

        public async Task<bool> DeleteAllImagesAsync(int chapterId, int currentUserId)
        {
            var chapter = await _chapterRepo.GetByIdAsync(chapterId);
            if (chapter == null) return false;
            if (!await OwnsStoryAsync(chapter.StoryId, currentUserId)) return false;

            await _storage.DeleteFolderAsync($"stories/{chapter.StoryId}/chapters/{chapterId}");
            await _chapterRepo.DeleteAllImagesAsync(chapterId);
            return true;
        }

        public async Task<bool> ReorderImagesAsync(int firstImageId, int currentUserId, List<int> orderedImageIds)
        {
            var first = await _chapterRepo.GetImageByIdAsync(firstImageId);
            if (first == null) return false;
            var chapter = await _chapterRepo.GetByIdAsync(first.ChapterId);
            if (chapter == null || !await OwnsStoryAsync(chapter.StoryId, currentUserId)) return false;

            await _chapterRepo.ReorderImagesAsync(first.ChapterId, orderedImageIds);
            return true;
        }

        public async Task<ChapterImageDto?> GetImageDtoAsync(int imageId, int currentUserId)
        {
            var image = await _chapterRepo.GetImageByIdAsync(imageId);
            if (image == null) return null;
            var chapter = await _chapterRepo.GetByIdAsync(image.ChapterId);
            if (chapter == null || !await OwnsStoryAsync(chapter.StoryId, currentUserId)) return null;
            return ChapterMapper.ToImageDto(image);
        }
    }
}