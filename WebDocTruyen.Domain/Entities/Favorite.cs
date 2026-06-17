using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDocTruyen.Domain.Entities
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int StoryId { get; set; }
        public Story Story { get; set; } = null!;
        public int? LastReadChapterId { get; set; }
        public Chapter? LastReadChapter { get; set; }
    }
}
