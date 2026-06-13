using WebDocTruyen.Application.DTOs.Comment;

namespace WebDocTruyen.Application.Interfaces
{
    public interface ICommentService
    {
        /// Bình luận của truyện (ChapterId == null)
        Task<List<CommentDto>> GetByStoryIdAsync(int storyId);

        /// Bình luận của 1 chapter cụ thể
        Task<List<CommentDto>> GetByChapterIdAsync(int chapterId);

        Task<CommentDto> AddAsync(int storyId, int userId, string content, int? chapterId = null);

        /// Trả về false nếu comment không tồn tại hoặc không có quyền xóa
        Task<bool> DeleteAsync(int commentId, int currentUserId, bool isAdmin);
    }
}