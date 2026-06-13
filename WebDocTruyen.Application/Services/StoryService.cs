using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepo;
        private readonly ICommentService _commentService;
        private readonly IFavoriteService _favoriteService;
        private readonly IRatingService _ratingService;
        private readonly IGenreRepository _genreRepo;

        public StoryService(
            IStoryRepository storyRepo,
            ICommentService commentService,
            IFavoriteService favoriteService,
            IRatingService ratingService,
            IGenreRepository genreRepo)
        {
            _storyRepo = storyRepo;
            _commentService = commentService;
            _favoriteService = favoriteService;
            _ratingService = ratingService;
            _genreRepo = genreRepo;
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

        // ── Chi tiết truyện (Details page) ───────────────────────────────
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

        // ── Form Create/Edit ───────────────────────────────────────────
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
                CreatedAt = DateTime.Now,
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
            existing.UpdatedAt = DateTime.Now;

            if (coverImageStream != null && !string.IsNullOrEmpty(coverImageFileName))
                existing.CoverImage = await SaveCoverImageAsync(existing.StoryId, coverImageStream, coverImageFileName);

            existing.StoryGenres = dto.SelectedGenreIds
                .Select(g => new StoryGenre { GenreId = g, StoryId = existing.StoryId }).ToList();

            await _storyRepo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int storyId, int currentUserId)
        {
            var story = await _storyRepo.GetByIdAsync(storyId);
            if (story == null) return false;
            if (story.CreatedBy != currentUserId) return false;

            await _storyRepo.DeleteAsync(storyId);

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stories", storyId.ToString());
            if (Directory.Exists(folder)) Directory.Delete(folder, true);

            return true;
        }

        // ── Helper ────────────────────────────────────────────────────
        private static async Task<string> SaveCoverImageAsync(int storyId, Stream content, string fileName)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stories", storyId.ToString());
            Directory.CreateDirectory(folder);
            var fn = Guid.NewGuid() + Path.GetExtension(fileName);
            using var fs = new FileStream(Path.Combine(folder, fn), FileMode.Create);
            await content.CopyToAsync(fs);
            return $"/images/stories/{storyId}/{fn}";
        }
    }
}