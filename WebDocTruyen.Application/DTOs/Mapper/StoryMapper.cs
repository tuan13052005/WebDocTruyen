using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Comment;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mapper
{
    public static class StoryMapper
    {
        public static StoryDto ToDto(Story s) => new()
        {
            Id = s.StoryId,
            Title = s.Title,
            Author = s.Author,
            Description = s.Description,
            Status = s.Status,
            CoverImage = string.IsNullOrEmpty(s.CoverImage) ? "/images/default.jpg" : s.CoverImage,
            ChapterCount = s.Chapters.Count,
            FavoriteCount = s.Favorites.Count,
            AvgRating = s.Ratings.Any(r => r.Score != null)
                            ? s.Ratings.Where(r => r.Score != null).Average(r => (double)r.Score!) : 0,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            Genres = s.StoryGenres.Select(sg => new GenreTagDto
            {
                GenreId = sg.GenreId,
                Name = sg.Genre?.Name ?? ""
            }).ToList()
        };

        public static StoryDetailDto ToDetailDto(Story s, List<CommentDto> comments,
            double avgRating, int ratingCount, int favCount, bool isFav, int? myRating,
            int? currentUserId, int? lastReadChapterId = null, int? lastReadChapterNumber = null) => new()
            {
                Id = s.StoryId,
                Title = s.Title,
                Author = s.Author,
                Description = s.Description,
                Status = s.Status,
                CoverImage = string.IsNullOrEmpty(s.CoverImage) ? "/images/default.jpg" : s.CoverImage,
                ChapterCount = s.Chapters.Count,
                FavoriteCount = favCount,
                AvgRating = avgRating,
                RatingCount = ratingCount,
                IsFavorited = isFav,
                MyRating = myRating,
                LastReadChapterId = lastReadChapterId,        // ➕
                LastReadChapterNumber = lastReadChapterId.HasValue
                    ? s.Chapters.FirstOrDefault(c => c.ChapterId == lastReadChapterId)?.ChapterNumber
                    : null,                                     // ➕
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                CreatorName = s.CreatedByUser?.Username ?? "",
                Genres = s.StoryGenres.Select(sg => new GenreTagDto
                {
                    GenreId = sg.GenreId,
                    Name = sg.Genre?.Name ?? ""
                }).ToList(),
                Chapters = s.Chapters.OrderBy(c => c.ChapterNumber).Select(c => new ChapterSummaryDto
                {
                    ChapterId = c.ChapterId,
                    ChapterNumber = c.ChapterNumber,
                    Title = c.Title,
                    ImageCount = c.ChapterImages.Count
                }).ToList(),
                Comments = comments
            };

        public static StoryFormDto ToFormDto(Story s) => new()
        {
            StoryId = s.StoryId,
            Title = s.Title,
            Author = s.Author,
            Description = s.Description,
            Status = s.Status,
            CoverImage = s.CoverImage,
            SelectedGenreIds = s.StoryGenres.Select(sg => sg.GenreId).ToList()
        };
    }
}