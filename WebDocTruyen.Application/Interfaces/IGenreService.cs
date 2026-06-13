using WebDocTruyen.Application.DTOs.Genre;

namespace WebDocTruyen.Application.Interfaces
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreDto>> GetAllAsync();
        Task<GenreDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(GenreDto dto);
        Task<bool> UpdateAsync(GenreDto dto);
        Task<bool> DeleteAsync(int id);
    }
}