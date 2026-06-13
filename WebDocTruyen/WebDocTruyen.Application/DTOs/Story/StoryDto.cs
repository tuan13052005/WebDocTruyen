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
}