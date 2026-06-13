using WebDocTruyen.Application.DTOs.Story;

namespace WebDocTruyen.Application.Interfaces
{
    /// <summary>
    /// Service layer cho Story — Controller chỉ phụ thuộc interface này,
    /// không gọi trực tiếp IStoryRepository (trả Entity).
    /// </summary>
    public interface IStoryService
    {
        Task<IEnumerable<StoryDto>> GetAllStoriesAsync();
        Task<StoryDto?> GetByIdAsync(int id);
        Task<IEnumerable<StoryDto>> SearchAsync(string keyword);
        Task<IEnumerable<StoryDto>> GetByGenreAsync(int genreId);

        // Chi tiết truyện (gồm chapters, comments, rating, favorite)
        Task<StoryDetailDto?> GetDetailAsync(int storyId, int? currentUserId);

        // CRUD (Editor)
        Task<StoryFormDto?> GetFormDtoAsync(int storyId);
        Task<int> CreateAsync(StoryFormDto dto, int createdBy, Stream? coverImageStream, string? coverImageFileName);
        Task<bool> UpdateAsync(StoryFormDto dto, int currentUserId, Stream? coverImageStream, string? coverImageFileName);
        Task<bool> DeleteAsync(int storyId, int currentUserId);
    }
}