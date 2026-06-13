using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Persistence;

namespace WebDocTruyen.Infrastructure.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly AppDbContext _context;

        public ChapterRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── Chapter ────────────────────────────────────────────────────────

        public async Task<Chapter?> GetByIdAsync(int id) =>
            await _context.Chapters.FindAsync(id);

        // Kèm ảnh, sắp xếp theo PageNumber
        public async Task<Chapter?> GetByIdWithImagesAsync(int id) =>
            await _context.Chapters
                .Include(c => c.ChapterImages.OrderBy(i => i.PageNumber))
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.ChapterId == id);

        public async Task<IEnumerable<Chapter>> GetByStoryIdAsync(int storyId) =>
            await _context.Chapters
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

        public async Task AddAsync(Chapter chapter)
        {
            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Chapter chapter)
        {
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.ChapterImages)
                .FirstOrDefaultAsync(c => c.ChapterId == id);
            if (chapter != null)
            {
                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Chapter?> GetByStoryAndNumberAsync(int storyId, int chapterNumber) =>
            await _context.Chapters
                .FirstOrDefaultAsync(c => c.StoryId == storyId && c.ChapterNumber == chapterNumber);

        public async Task<Story?> GetStoryWithChaptersAsync(int storyId) =>
            await _context.Stories
                .Include(s => s.Chapters.OrderBy(c => c.ChapterNumber))
                    .ThenInclude(c => c.ChapterImages)
                .FirstOrDefaultAsync(s => s.StoryId == storyId);

        // ── ChapterImage ───────────────────────────────────────────────────

        public async Task<IEnumerable<ChapterImage>> GetImagesByChapterIdAsync(int chapterId) =>
            await _context.ChapterImages
                .Where(i => i.ChapterId == chapterId)
                .OrderBy(i => i.PageNumber)
                .ToListAsync();

        public async Task<ChapterImage?> GetImageByIdAsync(int imageId) =>
            await _context.ChapterImages.FindAsync(imageId);

        // Upload 1 ảnh
        public async Task AddImageAsync(ChapterImage image)
        {
            _context.ChapterImages.Add(image);
            await _context.SaveChangesAsync();
        }

        // Upload nhiều ảnh cùng lúc (batch)
        public async Task AddImagesAsync(IEnumerable<ChapterImage> images)
        {
            _context.ChapterImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin ảnh (PageNumber, ImageUrl)
        public async Task UpdateImageAsync(ChapterImage image)
        {
            _context.ChapterImages.Update(image);
            await _context.SaveChangesAsync();
        }

        // Xóa 1 ảnh
        public async Task DeleteImageAsync(int imageId)
        {
            var image = await _context.ChapterImages.FindAsync(imageId);
            if (image != null)
            {
                _context.ChapterImages.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        // Xóa tất cả ảnh của chapter
        public async Task DeleteAllImagesAsync(int chapterId)
        {
            var images = await _context.ChapterImages
                .Where(i => i.ChapterId == chapterId)
                .ToListAsync();
            _context.ChapterImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        // Cập nhật lại thứ tự PageNumber theo danh sách ID được kéo thả
        public async Task ReorderImagesAsync(int chapterId, List<int> orderedImageIds)
        {
            var images = await _context.ChapterImages
                .Where(i => i.ChapterId == chapterId)
                .ToListAsync();

            for (int i = 0; i < orderedImageIds.Count; i++)
            {
                var img = images.FirstOrDefault(x => x.ImageId == orderedImageIds[i]);
                if (img != null)
                    img.PageNumber = i + 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}