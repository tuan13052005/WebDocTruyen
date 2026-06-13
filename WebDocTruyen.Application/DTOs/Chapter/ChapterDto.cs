using WebDocTruyen.Application.DTOs.Comment;

namespace WebDocTruyen.Application.DTOs.Chapter
{
    // Dùng cho form Add/Edit chapter
    public class ChapterFormDto
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public string StoryTitle { get; set; } = "";
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
    }

    // Dùng cho trang xác nhận xóa chapter
    public class ChapterDeleteDto
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public int ImageCount { get; set; }
    }
    public class ChapterImageDto
    {
        public int ImageId { get; set; }
        public int ChapterId { get; set; }
        public string ImageUrl { get; set; } = "";
        public int PageNumber { get; set; }
    }
    public class ChapterManageDto
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public string StoryTitle { get; set; } = "";
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public List<ChapterImageDto> Images { get; set; } = new();
    }
    public class ChapterReadDto
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public string StoryTitle { get; set; } = "";
        public string StoryCover { get; set; } = "";
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public string DisplayTitle => string.IsNullOrEmpty(Title)
            ? $"Chapter {ChapterNumber}"
            : $"Chapter {ChapterNumber}: {Title}";
        public int TotalImages { get; set; }
        public bool IsFavorited { get; set; }
        public int FavCount { get; set; }

        public List<ChapterImageDto> Images { get; set; } = new();
        public List<ChapterSummaryDto> AllChapters { get; set; } = new();
        public ChapterSummaryDto? PrevChapter { get; set; }
        public ChapterSummaryDto? NextChapter { get; set; }
        public List<CommentDto> Comments { get; set; } = new();
    }
    public class ChapterSummaryDto
    {
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public int ImageCount { get; set; }
        public string DisplayTitle => string.IsNullOrEmpty(Title)
            ? $"Chapter {ChapterNumber}"
            : $"Chapter {ChapterNumber}: {Title}";
    }
}