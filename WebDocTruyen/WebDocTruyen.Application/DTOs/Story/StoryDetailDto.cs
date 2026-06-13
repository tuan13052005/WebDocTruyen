using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDocTruyen.Application.DTOs.Chapter;
using WebDocTruyen.Application.DTOs.Comment;

namespace WebDocTruyen.Application.DTOs.Story
{
    public class StoryDetailDto : StoryDto
    {
        public int? CreatedBy { get; set; }
        public string CreatorName { get; set; } = "";
        public int RatingCount { get; set; }
        public int? MyRating { get; set; }
        public bool IsFavorited { get; set; }
        public List<ChapterSummaryDto> Chapters { get; set; } = new();
        public List<CommentDto> Comments { get; set; } = new();
    }
}
