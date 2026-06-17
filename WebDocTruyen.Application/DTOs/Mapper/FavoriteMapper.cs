using WebDocTruyen.Application.DTOs.Favorite;
using WebDocTruyen.Application.DTOs.Rating;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mapper
{
    public static class FavoriteMapper
    {
        public static FavoriteDto ToDto(Favorite f) => new()
        {
            FavoriteId = f.FavoriteId,
            UserId = f.UserId,
            StoryId = f.StoryId,
            StoryTitle = f.Story?.Title ?? "",
            CoverImage = string.IsNullOrEmpty(f.Story?.CoverImage) ? "/images/default.jpg" : f.Story!.CoverImage,
            Status = f.Story?.Status ?? "",
            ChapterCount = f.Story?.Chapters?.Count ?? 0,
            LastReadChapterId = f.LastReadChapterId,
            LastReadChapterNumber = f.LastReadChapter?.ChapterNumber
        };

        public static FavoriteStatusDto ToStatusDto(bool isFavorited, int count) => new()
        {
            IsFavorited = isFavorited,
            Count = count
        };
    }
}