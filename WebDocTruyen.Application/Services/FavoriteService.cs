using WebDocTruyen.Application.DTOs.Favorite;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepo;

        public FavoriteService(IFavoriteRepository favoriteRepo)
        {
            _favoriteRepo = favoriteRepo;
        }

        public Task<bool> IsFavoritedAsync(int userId, int storyId) =>
            _favoriteRepo.IsFavoritedAsync(userId, storyId);

        public Task<int> CountAsync(int storyId) =>
            _favoriteRepo.CountAsync(storyId);

        public async Task<FavoriteStatusDto> ToggleAsync(int userId, int storyId)
        {
            var existing = await _favoriteRepo.GetAsync(userId, storyId);

            if (existing != null)
                await _favoriteRepo.DeleteAsync(existing.FavoriteId);
            else
                await _favoriteRepo.AddAsync(new Favorite { UserId = userId, StoryId = storyId });

            int count = await _favoriteRepo.CountAsync(storyId);
            bool nowFavorited = existing == null;
            return FavoriteMapper.ToStatusDto(nowFavorited, count);
        }

        public async Task<List<FavoriteDto>> GetMyFavoritesAsync(int userId)
        {
            var favorites = await _favoriteRepo.GetByUserIdAsync(userId);
            return favorites.Select(FavoriteMapper.ToDto).ToList();
        }
    }
}