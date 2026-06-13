using WebDocTruyen.Application.DTOs.Rating;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mapper
{
    public static class RatingMapper
    {
        public static RatingDto ToDto(Rating r) => new()
        {
            RatingId = r.RatingId,
            UserId = r.UserId,
            Username = r.User?.Username ?? "",
            StoryId = r.StoryId,
            Score = r.Score,
            CreatedAt = r.CreatedAt
        };

        public static RatingResultDto ToResultDto(double avg, int count, int myScore) => new()
        {
            AvgRating = Math.Round(avg, 1),
            RatingCount = count,
            MyScore = myScore
        };

        public static RatingSummaryDto ToSummaryDto(int storyId, double avg, int count, int? myRating) => new()
        {
            StoryId = storyId,
            AvgRating = Math.Round(avg, 1),
            RatingCount = count,
            MyRating = myRating
        };
    }
}