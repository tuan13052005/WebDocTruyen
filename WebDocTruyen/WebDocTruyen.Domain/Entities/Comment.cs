using System;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Domain.Entities
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int StoryId { get; set; }
        public Story Story { get; set; } = null!;
        public int? ChapterId { get; set; }          // null = comment truyện, có giá trị = comment chapter
        public Chapter? Chapter { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}