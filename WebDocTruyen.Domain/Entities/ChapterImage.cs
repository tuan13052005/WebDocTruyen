namespace WebDocTruyen.Domain.Entities
{
    public class ChapterImage
    {
        public int ImageId { get; set; }
        public int ChapterId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int PageNumber { get; set; }

        // Navigation property
        public Chapter? Chapter { get; set; }
    }
}
