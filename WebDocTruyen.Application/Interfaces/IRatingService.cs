using WebDocTruyen.Application.DTOs.Rating;

namespace WebDocTruyen.Application.Interfaces
{
    public interface IRatingService
    {
        /// Lấy điểm trung bình, số lượt đánh giá, và điểm của user hiện tại (nếu có)
        Task<RatingSummaryDto> GetSummaryAsync(int storyId, int? userId);

        /// Chấm điểm (1-10), tự thêm mới hoặc cập nhật nếu user đã đánh giá truyện này
        Task<RatingResultDto> RateAsync(int userId, int storyId, int score);
    }
}