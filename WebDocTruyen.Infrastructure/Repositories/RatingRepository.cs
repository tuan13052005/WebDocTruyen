using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Persistence;

namespace WebDocTruyen.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _ctx;
        public RatingRepository(AppDbContext ctx) { _ctx = ctx; }

        public async Task<Rating?> GetByUserAndStoryAsync(int userId, int storyId) =>
            await _ctx.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.StoryId == storyId);

        public async Task AddAsync(Rating rating)
        {
            _ctx.Ratings.Add(rating);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rating rating)
        {
            _ctx.Ratings.Update(rating);
            await _ctx.SaveChangesAsync();
        }

        public async Task<double> GetAverageAsync(int storyId)
        {
            var scores = await _ctx.Ratings
                .Where(r => r.StoryId == storyId && r.Score != null)
                .Select(r => (int)r.Score!)
                .ToListAsync();
            return scores.Any() ? scores.Average() : 0;
        }

        public async Task<int> CountAsync(int storyId) =>
            await _ctx.Ratings.CountAsync(r => r.StoryId == storyId && r.Score != null);
    }
}