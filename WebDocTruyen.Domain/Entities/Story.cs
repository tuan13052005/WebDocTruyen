namespace WebDocTruyen.Domain.Entities
{
    public class Story
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "ongoing";
        public string CoverImage { get; set; } = "/images/default.jpg";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User? CreatedByUser { get; set; }

        // ⚡ Thêm property này để fix lỗi
        public ICollection<StoryGenre> StoryGenres { get; set; } = new List<StoryGenre>();

        // Nếu có Chapters, Comments, Ratings, Favorites thì cũng thêm:
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public string NormalizedTitle { get; set; } = "";
        public string NormalizedAuthor { get; set; } = "";
    }
}
