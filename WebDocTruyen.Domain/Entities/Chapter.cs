namespace WebDocTruyen.Domain.Entities
{
    public class Chapter
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public Story? Story { get; set; }
        public string Title { get; set; } = "";
        public int ChapterNumber { get; set; }

        public ICollection<ChapterImage> ChapterImages { get; set; } = new List<ChapterImage>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}