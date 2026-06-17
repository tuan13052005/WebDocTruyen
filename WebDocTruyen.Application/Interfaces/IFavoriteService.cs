using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Favorite;
using WebDocTruyen.Application.DTOs.Rating;
using WebDocTruyen.Application.DTOs.Story;

namespace WebDocTruyen.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<bool> IsFavoritedAsync(int userId, int storyId);
        Task<int> CountAsync(int storyId);

        /// Toggle theo dõi, trả về trạng thái mới + tổng số lượt theo dõi
        Task<FavoriteStatusDto> ToggleAsync(int userId, int storyId);

        /// Danh sách truyện user đang theo dõi
        Task<List<FavoriteDto>> GetMyFavoritesAsync(int userId);
        Task MarkReadingAsync(int userId, int storyId, int chapterId);
    }
}