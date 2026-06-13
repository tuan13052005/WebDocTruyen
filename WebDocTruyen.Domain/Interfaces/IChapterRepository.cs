using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Domain.Interfaces
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int id);
        Task<Chapter?> GetByIdWithImagesAsync(int id);   // kèm ChapterImages
        Task<IEnumerable<Chapter>> GetByStoryIdAsync(int storyId);
        Task AddAsync(Chapter chapter);
        Task UpdateAsync(Chapter chapter);
        Task DeleteAsync(int id);

        // ChapterImage
        Task AddImageAsync(ChapterImage image);
        Task AddImagesAsync(IEnumerable<ChapterImage> images);
        Task<IEnumerable<ChapterImage>> GetImagesByChapterIdAsync(int chapterId);
        Task<ChapterImage?> GetImageByIdAsync(int imageId);
        Task UpdateImageAsync(ChapterImage image);
        Task DeleteImageAsync(int imageId);
        Task DeleteAllImagesAsync(int chapterId);
        Task ReorderImagesAsync(int chapterId, List<int> orderedImageIds);

        Task<Chapter?> GetByStoryAndNumberAsync(int storyId, int chapterNumber);
        Task<Story?> GetStoryWithChaptersAsync(int storyId);
    }
}