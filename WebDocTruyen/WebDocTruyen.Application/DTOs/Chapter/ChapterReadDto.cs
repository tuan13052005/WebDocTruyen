using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDocTruyen.Application.DTOs.Comment;

namespace WebDocTruyen.Application.DTOs.Chapter
{
    public class ChapterReadDto
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public string StoryTitle { get; set; } = "";
        public string StoryCover { get; set; } = "";
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public string DisplayTitle => string.IsNullOrEmpty(Title)
            ? $"Chapter {ChapterNumber}"
            : $"Chapter {ChapterNumber}: {Title}";
        public int TotalImages { get; set; }
        public bool IsFavorited { get; set; }
        public int FavCount { get; set; }

        public List<ChapterImageDto> Images { get; set; } = new();
        public List<ChapterSummaryDto> AllChapters { get; set; } = new();
        public ChapterSummaryDto? PrevChapter { get; set; }
        public ChapterSummaryDto? NextChapter { get; set; }
        public List<CommentDto> Comments { get; set; } = new();
    }
}
