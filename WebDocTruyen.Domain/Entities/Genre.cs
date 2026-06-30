namespace WebDocTruyen.Domain.Entities
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Quan hệ nhiều-nhiều với Stories qua StoryGenres
        public ICollection<StoryGenre> StoryGenres { get; set; } = new List<StoryGenre>();
    }
}
