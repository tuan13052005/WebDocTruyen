using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Domain.Interfaces
{
    public interface IRatingRepository
    {
        Task<Rating?> GetByUserAndStoryAsync(int userId, int storyId);
        Task AddAsync(Rating rating);
        Task UpdateAsync(Rating rating);
        Task<double> GetAverageAsync(int storyId);
        Task<int> CountAsync(int storyId);
    }
}