using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Application.DTOs.Chapter
{
    public class ChapterImageDto
    {
        public int ImageId { get; set; }
        public int ChapterId { get; set; }
        public string ImageUrl { get; set; } = "";
        public int PageNumber { get; set; }
    }
}
