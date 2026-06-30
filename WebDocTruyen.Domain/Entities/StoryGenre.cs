namespace WebDocTruyen.Domain.Entities
{
    public class StoryGenre
    {
        public int StoryGenreId { get; set; }

        // Khóa ngoại
        public int StoryId { get; set; }
        public int GenreId { get; set; }

        // Navigation properties
        public Story? Story { get; set; }
        public Genre? Genre { get; set; }
    }
}
