using Microsoft.EntityFrameworkCore;
using System.Text;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Persistence;

namespace WebDocTruyen.Infrastructure.Repositories
{
    public class StoryRepository : IStoryRepository
    {
        private readonly AppDbContext _context;

        public StoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(int id)
        {
            // Load đầy đủ tất cả bảng con có FK trỏ về Stories
            var story = await _context.Stories
                .Include(s => s.StoryGenres)
                .Include(s => s.Favorites)
                .Include(s => s.Ratings)
                .Include(s => s.Comments)
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.ChapterImages)
                .FirstOrDefaultAsync(s => s.StoryId == id);

            if (story == null) return;

            // Xóa theo thứ tự: con trước, cha sau
            foreach (var chapter in story.Chapters)
                _context.ChapterImages.RemoveRange(chapter.ChapterImages);

            _context.Chapters.RemoveRange(story.Chapters);
            _context.StoryGenres.RemoveRange(story.StoryGenres);
            _context.Favorites.RemoveRange(story.Favorites);
            _context.Ratings.RemoveRange(story.Ratings);
            _context.Comments.RemoveRange(story.Comments);

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Story>> GetAllAsync()
        {
            return await _context.Stories
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .Include(s => s.Chapters)
                .ToListAsync();
        }

        public IEnumerable<Story> GetByGenre(int genreId)
        {
            return _context.Stories
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .Where(s => s.StoryGenres.Any(sg => sg.GenreId == genreId))
                .ToList();
        }

        public async Task<Story?> GetByIdAsync(int id)
        {
            return await _context.Stories
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .Include(s => s.Chapters)
                .Include(s => s.Comments)
                .Include(s => s.Ratings)
                .Include(s => s.Favorites)
                .FirstOrDefaultAsync(s => s.StoryId == id);
        }

        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // Bỏ dấu
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // Chuyển về chữ thường, thay khoảng trắng bằng '-'
            var slug = sb.ToString().Normalize(System.Text.NormalizationForm.FormC)
                .ToLower()
                .Replace(" ", "-");

            // Loại bỏ ký tự đặc biệt
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

            return slug;
        }

        public async Task AddAsync(Story story)
        {
            // Tạo slug cho Title và Author trước khi lưu
            story.NormalizedTitle = GenerateSlug(story.Title);
            story.NormalizedAuthor = GenerateSlug(story.Author);

            await _context.Stories.AddAsync(story);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Story story)
        {
            story.NormalizedTitle = GenerateSlug(story.Title);
            story.NormalizedAuthor = GenerateSlug(story.Author);

            // Xóa StoryGenres cũ trong DB trước
            var oldGenres = await _context.StoryGenres
                .Where(sg => sg.StoryId == story.StoryId)
                .ToListAsync();
            _context.StoryGenres.RemoveRange(oldGenres);

            _context.Stories.Update(story);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Story>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Story>();

            // Chuẩn hóa keyword: bỏ dấu + lowercase
            string normalizedKeyword = GenerateSlug(keyword);

            return await _context.Stories
                .Where(s => s.NormalizedTitle.Contains(normalizedKeyword) ||
                            s.NormalizedAuthor.Contains(normalizedKeyword))
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .Include(s => s.Chapters)
                .ToListAsync();
        }
    }
}