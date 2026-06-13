using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Application.DTOs.Genre
{
    public class GenreDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int StoryCount { get; set; }
    }
}
