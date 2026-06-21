using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Comment;

namespace WebDocTruyen.Application.DTOs.Story
{
    public class StoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public string CoverImage { get; set; } = "";
        public int ChapterCount { get; set; }
        public int FavoriteCount { get; set; }
        public double AvgRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<GenreTagDto> Genres { get; set; } = new();
    }
    public class StoryDetailDto : StoryDto
    {
        public int? CreatedBy { get; set; }
        public string CreatorName { get; set; } = "";
        public int RatingCount { get; set; }
        public int? MyRating { get; set; }
        public bool IsFavorited { get; set; }
        public int? LastReadChapterId { get; set; }
        public int? LastReadChapterNumber { get; set; }
        public List<ChapterSummaryDto> Chapters { get; set; } = new();
        public List<CommentDto> Comments { get; set; } = new();
    }
    public class GenreTagDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = "";
    }
    public class StoryFormDto
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "ongoing";
        public string CoverImage { get; set; } = "";
        public List<int> SelectedGenreIds { get; set; } = new();
    }
}