using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Linq;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Persistence;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebDocTruyen.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _ctx;
        public CommentRepository(AppDbContext ctx) { _ctx = ctx; }

        // Comment của truyện (không thuộc chapter nào)
        public async Task<IEnumerable<Comment>> GetByStoryIdAsync(int storyId) =>
            await _ctx.Comments
                .Include(c => c.User)
                .Where(c => c.StoryId == storyId && c.ChapterId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        // Comment của chapter cụ thể
        public async Task<IEnumerable<Comment>> GetByChapterIdAsync(int chapterId) =>
            await _ctx.Comments
                .Include(c => c.User)
                .Where(c => c.ChapterId == chapterId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task AddAsync(Comment comment)
        {
            _ctx.Comments.Add(comment);
            await _ctx.SaveChangesAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id) =>
            await _ctx.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.CommentId == id);

        public async Task DeleteAsync(int id)
        {
            var c = await _ctx.Comments.FindAsync(id);
            if (c != null) { _ctx.Comments.Remove(c); await _ctx.SaveChangesAsync(); }
        }
    }
}