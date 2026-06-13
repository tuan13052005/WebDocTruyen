using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Domain.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetByStoryIdAsync(int storyId);
        Task<IEnumerable<Comment>> GetByChapterIdAsync(int chapterId);
        Task AddAsync(Comment comment);
        Task<Comment?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}