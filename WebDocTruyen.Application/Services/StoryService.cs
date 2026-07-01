// WebDocTruyen.Application/Services/StoryService.cs
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Storage;

namespace WebDocTruyen.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepo;
        private readonly ICommentService _commentService;
        private readonly IFavoriteService _favoriteService;
        private readonly IRatingService _ratingService;
        private readonly IGenreRepository _genreRepo;
        private readonly ISupabaseStorageService _storage;

        public StoryService(
            IStoryRepository storyRepo,
            ICommentService commentService,
            IFavoriteService favoriteService,
            IRatingService ratingService,
            IGenreRepository genreRepo,
            ISupabaseStorageService storage)
        {
            _storyRepo = storyRepo;
            _commentService = commentService;
            _favoriteService = favoriteService;
            _ratingService = ratingService;
            _genreRepo = genreRepo;
            _storage = storage;
        }

        public async Task<IEnumerable<StoryDto>> GetAllStoriesAsync()
        {
            var stories = await _storyRepo.GetAllAsync();
            return stories.Select(StoryMapper.ToDto);
        }

        public async Task<StoryDto?> GetByIdAsync(int id)
        {
            var s = await _storyRepo.GetByIdAsync(id);
            return s == null ? null : StoryMapper.ToDto(s);
        }

        public async Task<IEnumerable<StoryDto>> SearchAsync(string keyword)
        {
            var stories = await _storyRepo.SearchAsync(keyword);
            return stories.Select(StoryMapper.ToDto);
        }

        public async Task<IEnumerable<StoryDto>> GetByGenreAsync(int genreId)
        {
            var stories = _storyRepo.GetByGenre(genreId);
            return stories.Select(StoryMapper.ToDto);
        }

        public async Task<StoryDetailDto?> GetDetailAsync(int storyId, int? currentUserId)
        {
            var story = await _storyRepo.GetByIdAsync(storyId);
            if (story == null) return null;

            var comments = await _commentService.GetByStoryIdAsync(storyId);
            var ratingInfo = await _ratingService.GetSummaryAsync(storyId, currentUserId);
            var favCount = await _favoriteService.CountAsync(storyId);
            bool isFav = currentUserId.HasValue
                && await _favoriteService.IsFavoritedAsync(currentUserId.Value, storyId);

            return StoryMapper.ToDetailDto(
                story, comments,
                ratingInfo.AvgRating, ratingInfo.RatingCount,
                favCount, isFav, ratingInfo.MyRating, currentUserId);
        }

        public async Task<StoryFormDto?> GetFormDtoAsync(int storyId)
        {
            var story = await _storyRepo.GetByIdAsync(storyId);
            return story == null ? null : StoryMapper.ToFormDto(story);
        }

        public async Task<int> CreateAsync(StoryFormDto dto, int createdBy,
            Stream? coverImageStream, string? coverImageFileName)
        {
            var story = new Story
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                StoryGenres = new List<StoryGenre>()
            };
            await _storyRepo.AddAsync(story);

            if (coverImageStream != null && !string.IsNullOrEmpty(coverImageFileName))
                story.CoverImage = await SaveCoverImageAsync(story.StoryId, coverImageStream, coverImageFileName);

            story.StoryGenres = dto.SelectedGenreIds
                .Select(g => new StoryGenre { GenreId = g, StoryId = story.StoryId }).ToList();
            await _storyRepo.UpdateAsync(story);

            return story.StoryId;
        }

        public async Task<bool> UpdateAsync(StoryFormDto dto, int currentUserId,
            Stream? coverImageStream, string? coverImageFileName)
        {
            var existing = await _storyRepo.GetByIdAsync(dto.StoryId);
            if (existing == null) return false;
            if (existing.CreatedBy != currentUserId) return false;

            existing.Title = dto.Title;
            existing.Author = dto.Author;
            existing.Description = dto.Description;
            existing.Status = dto.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            if (coverImageStream != null && !string.IsNullOrEmpty(coverImageFileName))
            {
                // Xóa ảnh bìa cũ trên Supabase (nếu có)
                var oldPath = _storage.GetPathFromPublicUrl(existing.CoverImage);
                if (oldPath != null) await _storage.DeleteAsync(oldPath);

                existing.CoverImage = await SaveCoverImageAsync(existing.StoryId, coverImageStream, coverImageFileName);
            }

            var toRemove = existing.StoryGenres
                .Where(sg => !dto.SelectedGenreIds.Contains(sg.GenreId))
                .ToList();
            foreach (var sg in toRemove)
                existing.StoryGenres.Remove(sg);

            var currentGenreIds = existing.StoryGenres.Select(sg => sg.GenreId).ToList();
            var toAdd = dto.SelectedGenreIds
                .Where(id => !currentGenreIds.Contains(id))
                .Select(id => new StoryGenre { StoryId = existing.StoryId, GenreId = id });
            foreach (var sg in toAdd)
                existing.StoryGenres.Add(sg);

            await _storyRepo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int storyId, int currentUserId)
        {
            var story = await _storyRepo.GetByIdAsync(storyId);
            if (story == null) return false;
            if (story.CreatedBy != currentUserId) return false;

            await _storyRepo.DeleteAsync(storyId);

            // Xóa toàn bộ ảnh của truyện trên Supabase (bìa + ảnh chapter)
            await _storage.DeleteFolderAsync($"stories/{storyId}");

            return true;
        }

        // ── Helper ────────────────────────────────────────────────────
        private async Task<string> SaveCoverImageAsync(int storyId, Stream content, string fileName)
        {
            var ext = Path.GetExtension(fileName);
            var path = $"stories/{storyId}/cover/{Guid.NewGuid()}{ext}";
            var contentType = _storage.GetContentType(fileName);
            return await _storage.UploadAsync(content, path, contentType);
        }
    }
}