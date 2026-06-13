using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Persistence;

namespace WebDocTruyen.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _ctx;
        public FavoriteRepository(AppDbContext ctx) { _ctx = ctx; }

        public async Task<bool> IsFavoritedAsync(int userId, int storyId) =>
            await _ctx.Favorites.AnyAsync(f => f.UserId == userId && f.StoryId == storyId);

        public async Task<Favorite?> GetAsync(int userId, int storyId) =>
            await _ctx.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.StoryId == storyId);

        public async Task AddAsync(Favorite favorite)
        {
            _ctx.Favorites.Add(favorite);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int favoriteId)
        {
            var f = await _ctx.Favorites.FindAsync(favoriteId);
            if (f != null) { _ctx.Favorites.Remove(f); await _ctx.SaveChangesAsync(); }
        }

        public async Task<int> CountAsync(int storyId) =>
            await _ctx.Favorites.CountAsync(f => f.StoryId == storyId);

        public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId) =>
            await _ctx.Favorites
                .Include(f => f.Story)
                    .ThenInclude(s => s.Chapters)
                .Where(f => f.UserId == userId)
                .ToListAsync();
    }
}