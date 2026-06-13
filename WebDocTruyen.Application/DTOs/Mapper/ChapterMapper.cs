using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Comment;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mappers
{
    public static class ChapterMapper
    {
        public static ChapterSummaryDto ToSummary(Chapter c) => new()
        {
            ChapterId = c.ChapterId,
            ChapterNumber = c.ChapterNumber,
            Title = c.Title,
            ImageCount = c.ChapterImages.Count
        };

        public static ChapterImageDto ToImageDto(ChapterImage img) => new()
        {
            ImageId = img.ImageId,
            ChapterId = img.ChapterId,
            ImageUrl = img.ImageUrl,
            PageNumber = img.PageNumber
        };

        public static ChapterReadDto ToReadDto(
            Chapter chapter,
            Story? story,
            List<ChapterImageDto> images,
            List<ChapterSummaryDto> allChapters,
            ChapterSummaryDto? prev,
            ChapterSummaryDto? next,
            List<CommentDto> comments,
            bool isFav, int favCount) => new()
            {
                ChapterId = chapter.ChapterId,
                StoryId = chapter.StoryId,
                StoryTitle = story?.Title ?? "",
                StoryCover = story?.CoverImage ?? "/images/default.jpg",
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                TotalImages = images.Count,
                IsFavorited = isFav,
                FavCount = favCount,
                Images = images,
                AllChapters = allChapters,
                PrevChapter = prev,
                NextChapter = next,
                Comments = comments
            };

        public static ChapterManageDto ToManageDto(Chapter chapter, Story? story) => new()
        {
            ChapterId = chapter.ChapterId,
            StoryId = chapter.StoryId,
            StoryTitle = story?.Title ?? "",
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Images = chapter.ChapterImages
                .OrderBy(i => i.PageNumber)
                .Select(ToImageDto)
                .ToList()
        };
        public static ChapterFormDto ToFormDto(Chapter chapter, Story? story) => new()
        {
            ChapterId = chapter.ChapterId,
            StoryId = chapter.StoryId,
            StoryTitle = story?.Title ?? "",
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title
        };

        public static ChapterDeleteDto ToDeleteDto(Chapter chapter) => new()
        {
            ChapterId = chapter.ChapterId,
            StoryId = chapter.StoryId,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            ImageCount = chapter.ChapterImages.Count
        };
    }
}