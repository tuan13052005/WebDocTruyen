using WebDocTruyen.Application.DTOs.Genre;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Application.Mapper
{
    public static class GenreMapper
    {
        public static GenreDto ToDto(Genre g) => new()
        {
            GenreId = g.GenreId,
            Name = g.Name,
            Description = g.Description ?? "",
            StoryCount = g.StoryGenres.Count
        };
    }
}