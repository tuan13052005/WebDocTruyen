using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Application.DTOs.Chapter
{
    public class ChapterSummaryDto
    {
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public int ImageCount { get; set; }
        public string DisplayTitle => string.IsNullOrEmpty(Title)
            ? $"Chapter {ChapterNumber}"
            : $"Chapter {ChapterNumber}: {Title}";
    }
}
