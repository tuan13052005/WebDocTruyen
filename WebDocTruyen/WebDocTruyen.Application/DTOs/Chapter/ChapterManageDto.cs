using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Application.DTOs.Chapter
{
    public class ChapterManageDto
    {
        public int ChapterId { get; set; }
        public int StoryId { get; set; }
        public string StoryTitle { get; set; } = "";
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = "";
        public List<ChapterImageDto> Images { get; set; } = new();
    }
}
