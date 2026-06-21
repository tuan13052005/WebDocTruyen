using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Story> Stories { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<StoryGenre> StoryGenres { get; set; } = null!;
        public DbSet<ChapterImage> ChapterImages { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Story>(e =>
            {
                e.HasKey(s => s.StoryId);
                e.HasOne(s => s.CreatedByUser).WithMany().HasForeignKey(s => s.CreatedBy);
            });

            mb.Entity<Genre>(e =>
            {
                e.HasKey(g => g.GenreId);
                e.Property(g => g.Name).IsRequired().HasMaxLength(100);
            });

            mb.Entity<StoryGenre>(e =>
            {
                e.HasKey(sg => sg.StoryGenreId);
                e.HasOne(sg => sg.Story)
                    .WithMany(s => s.StoryGenres)
                    .HasForeignKey(sg => sg.StoryId)
                    .OnDelete(DeleteBehavior.Cascade); 
                e.HasOne(sg => sg.Genre)
                    .WithMany(g => g.StoryGenres)
                    .HasForeignKey(sg => sg.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<Comment>(e =>
            {
                e.HasKey(c => c.CommentId);
                e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
                e.HasOne(c => c.Story).WithMany(s => s.Comments).HasForeignKey(c => c.StoryId);
                // FIX: thêm relation Comment → Chapter để GetByChapterIdAsync hoạt động
                e.HasOne(c => c.Chapter)
                 .WithMany(ch => ch.Comments)
                 .HasForeignKey(c => c.ChapterId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<Favorite>(e =>
            {
                e.HasKey(f => f.FavoriteId);
                e.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId);
                e.HasOne(f => f.Story).WithMany(s => s.Favorites).HasForeignKey(f => f.StoryId);

                e.HasOne(f => f.LastReadChapter)
                 .WithMany()
                 .HasForeignKey(f => f.LastReadChapterId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            mb.Entity<Chapter>()
                .HasMany(c => c.ChapterImages)
                .WithOne(ci => ci.Chapter)
                .HasForeignKey(ci => ci.ChapterId);

            mb.Entity<ChapterImage>(e =>
            {
                e.HasKey(ci => ci.ImageId);
                e.Property(ci => ci.ImageUrl).IsRequired();
                e.HasOne(ci => ci.Chapter).WithMany(c => c.ChapterImages).HasForeignKey(ci => ci.ChapterId);
            });

            mb.Entity<Rating>(e =>
            {
                e.HasKey(r => r.RatingId);
                e.HasOne(r => r.User).WithMany(u => u.Ratings).HasForeignKey(r => r.UserId);
                e.HasOne(r => r.Story).WithMany(s => s.Ratings).HasForeignKey(r => r.StoryId);
            });

            mb.Entity<User>(e => e.HasKey(u => u.UserId));
        }
    }
}