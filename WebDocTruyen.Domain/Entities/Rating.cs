namespace WebDocTruyen.Domain.Entities
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public int StoryId { get; set; }
        public int? Score { get; set; }   // có thể null
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Story? Story { get; set; }
    }
}
