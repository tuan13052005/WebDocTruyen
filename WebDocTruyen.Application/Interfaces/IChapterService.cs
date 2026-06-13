using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Story;

namespace WebDocTruyen.Application.Interfaces
{
    public interface IChapterService
    {
        /// <summary>Danh sách chapter của 1 truyện (có phân trang)</summary>
        Task<(List<ChapterSummaryDto> Items, int Total, StoryDto? Story)> GetListAsync(
            int storyId, int page, int pageSize);

        /// <summary>Trang đọc chapter: ảnh, prev/next, comment, favorite</summary>
        Task<ChapterReadDto?> GetReadDtoAsync(int chapterId, int? currentUserId);

        // ── Editor: CRUD chapter ──────────────────────────────────
        Task<ChapterFormDto?> GetFormDtoAsync(int chapterId);
        Task<int> CreateAsync(int storyId, ChapterFormDto dto, int currentUserId);
        Task<bool> UpdateAsync(ChapterFormDto dto, int currentUserId);
        Task<ChapterDeleteDto?> GetDeleteDtoAsync(int chapterId, int currentUserId);
        Task<bool> DeleteAsync(int chapterId, int currentUserId);

        // ── Editor: CRUD ảnh ──────────────────────────────────────
        Task<ChapterManageDto?> GetManageDtoAsync(int chapterId, int currentUserId);
        Task<int> UploadImagesAsync(int chapterId, int currentUserId,
            IEnumerable<(Stream Content, string FileName, long Length)> files);
        Task<bool> UpdateImageAsync(int imageId, int currentUserId, int pageNumber,
            (Stream Content, string FileName)? newImage);
        Task<bool> DeleteImageAsync(int imageId, int currentUserId);
        Task<bool> DeleteAllImagesAsync(int chapterId, int currentUserId);
        Task<bool> ReorderImagesAsync(int firstImageId, int currentUserId, List<int> orderedImageIds);
        Task<ChapterImageDto?> GetImageDtoAsync(int imageId, int currentUserId);
    }
}