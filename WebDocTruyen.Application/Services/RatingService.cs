using WebDocTruyen.Application.DTOs.Rating;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepo;

        public RatingService(IRatingRepository ratingRepo)
        {
            _ratingRepo = ratingRepo;
        }

        public async Task<RatingSummaryDto> GetSummaryAsync(int storyId, int? userId)
        {
            double avg = await _ratingRepo.GetAverageAsync(storyId);
            int count = await _ratingRepo.CountAsync(storyId);
            int? mine = null;

            if (userId.HasValue)
            {
                var r = await _ratingRepo.GetByUserAndStoryAsync(userId.Value, storyId);
                mine = r?.Score;
            }

            return RatingMapper.ToSummaryDto(storyId, avg, count, mine);
        }

        public async Task<RatingResultDto> RateAsync(int userId, int storyId, int score)
        {
            if (score < 1 || score > 10)
                throw new ArgumentOutOfRangeException(nameof(score), "Điểm phải từ 1 đến 10");

            var existing = await _ratingRepo.GetByUserAndStoryAsync(userId, storyId);

            if (existing != null)
            {
                existing.Score = score;
                await _ratingRepo.UpdateAsync(existing);
            }
            else
            {
                await _ratingRepo.AddAsync(new Rating
                {
                    UserId = userId,
                    StoryId = storyId,
                    Score = score,
                    CreatedAt = DateTime.Now
                });
            }

            double avg = await _ratingRepo.GetAverageAsync(storyId);
            int count = await _ratingRepo.CountAsync(storyId);
            return RatingMapper.ToResultDto(avg, count, score);
        }
    }
}