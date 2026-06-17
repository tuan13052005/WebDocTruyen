namespace WebDocTruyen.Application.DTOs.Favorite
{
    // Dùng khi trả về kết quả toggle favorite (AJAX) và khi hiển thị
    // danh sách "Truyện đã theo dõi" của user
    public class FavoriteDto
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int StoryId { get; set; }
        public string StoryTitle { get; set; } = "";
        public string CoverImage { get; set; } = "";
        public string Status { get; set; } = "";
        public int ChapterCount { get; set; }
        public int? LastReadChapterId { get; set; }
        public int? LastReadChapterNumber { get; set; }
    }

    // Kết quả trả về cho AJAX ToggleFavorite
    public class FavoriteStatusDto
    {
        public bool IsFavorited { get; set; }
        public int Count { get; set; }
    }
}