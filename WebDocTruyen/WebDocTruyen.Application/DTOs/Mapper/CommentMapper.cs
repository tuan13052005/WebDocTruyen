using WebDocTruyen.Application.DTOs.Comment;
using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mappers
{
    public static class CommentMapper
    {
        public static CommentDto ToDto(Comment c) => new()
        {
            CommentId = c.CommentId,
            UserId = c.UserId,
            Username = c.User?.Username ?? "",
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            ChapterId = c.ChapterId,
            StoryId = c.StoryId
        };
    }
}