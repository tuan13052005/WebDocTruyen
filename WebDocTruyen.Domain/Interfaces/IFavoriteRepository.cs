using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Domain.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<bool> IsFavoritedAsync(int userId, int storyId);
        Task<Favorite?> GetAsync(int userId, int storyId);
        Task AddAsync(Favorite favorite);
        Task DeleteAsync(int favoriteId);
        Task<int> CountAsync(int storyId);
        Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
        Task UpdateLastReadChapterAsync(int userId, int storyId, int chapterId);
    }
}