namespace WebDocTruyen.Application.DTOs.Rating
{
    // Thông tin đánh giá của 1 user cho 1 truyện
    public class RatingDto
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public int StoryId { get; set; }
        public int? Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Kết quả trả về cho AJAX Rate (điểm trung bình + điểm vừa chấm)
    public class RatingResultDto
    {
        public double AvgRating { get; set; }
        public int RatingCount { get; set; }
        public int MyScore { get; set; }
    }

    // Tổng hợp đánh giá của 1 truyện (dùng trong StoryDetailDto nếu cần)
    public class RatingSummaryDto
    {
        public int StoryId { get; set; }
        public double AvgRating { get; set; }
        public int RatingCount { get; set; }
        public int? MyRating { get; set; }
    }
}