using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Application.DTOs.Story
{
    public class StoryFormDto
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "ongoing";
        public string CoverImage { get; set; } = "";
        public List<int> SelectedGenreIds { get; set; } = new();
    }
}
