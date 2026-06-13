using WebDocTruyen.Application.DTOs.Comment;

namespace WebDocTruyen.Application.Interfaces
{
    public interface ICommentService
    {
        /// <summary>Bình luận cấp truyện (ChapterId == null)</summary>
        Task<List<CommentDto>> GetByStoryIdAsync(int storyId);

        /// <summary>Bình luận của 1 chapter cụ thể</summary>
        Task<List<CommentDto>> GetByChapterIdAsync(int chapterId);

        Task<CommentDto?> GetByIdAsync(int commentId);

        Task<CommentDto> AddAsync(int storyId, int userId, string content, int? chapterId = null);

        /// <summary>Trả về false nếu comment không tồn tại hoặc không có quyền xóa</summary>
        Task<bool> DeleteAsync(int commentId, int currentUserId, bool isAdmin);
    }
}