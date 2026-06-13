using WebDocTruyen.Application.DTOs.Comment;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepo;

        public CommentService(ICommentRepository commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<List<CommentDto>> GetByStoryIdAsync(int storyId)
        {
            var comments = await _commentRepo.GetByStoryIdAsync(storyId);
            return comments.Select(CommentMapper.ToDto).ToList();
        }

        public async Task<List<CommentDto>> GetByChapterIdAsync(int chapterId)
        {
            var comments = await _commentRepo.GetByChapterIdAsync(chapterId);
            return comments.Select(CommentMapper.ToDto).ToList();
        }

        public async Task<CommentDto?> GetByIdAsync(int commentId)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            return comment == null ? null : CommentMapper.ToDto(comment);
        }

        public async Task<CommentDto> AddAsync(int storyId, int userId, string content, int? chapterId = null)
        {
            var comment = new Comment
            {
                StoryId = storyId,
                ChapterId = chapterId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };
            await _commentRepo.AddAsync(comment);

            // Lấy lại để có thông tin User (Username) cho DTO
            var saved = await _commentRepo.GetByIdAsync(comment.CommentId);
            return CommentMapper.ToDto(saved ?? comment);
        }

        public async Task<bool> DeleteAsync(int commentId, int currentUserId, bool isAdmin)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null) return false;
            if (comment.UserId != currentUserId && !isAdmin) return false;

            await _commentRepo.DeleteAsync(commentId);
            return true;
        }
    }
}