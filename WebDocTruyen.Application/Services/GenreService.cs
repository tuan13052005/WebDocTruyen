using WebDocTruyen.Application.DTOs.Genre;
using WebDocTruyen.Application.Interfaces;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepo;

        public GenreService(IGenreRepository genreRepo)
        {
            _genreRepo = genreRepo;
        }

        public async Task<IEnumerable<GenreDto>> GetAllAsync()
        {
            var genres = await _genreRepo.GetAllGenreAsync();
            return genres.Select(GenreMapper.ToDto);
        }

        public async Task<GenreDto?> GetByIdAsync(int id)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            return genre == null ? null : GenreMapper.ToDto(genre);
        }

        public Task<int> CreateAsync(GenreDto dto)
        {
            var genre = new Genre
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            };
            _genreRepo.Add(genre);
            return Task.FromResult(genre.GenreId);
        }

        public async Task<bool> UpdateAsync(GenreDto dto)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(dto.GenreId);
            if (genre == null) return false;

            genre.Name = dto.Name.Trim();
            genre.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            _genreRepo.Update(genre);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return false;

            _genreRepo.Delete(id);
            return true;
        }
    }
}