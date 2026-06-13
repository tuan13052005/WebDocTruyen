using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Infrastructure.Persistence;

namespace WebDocTruyen.Infrastructure.Repositories
{
    public class GenreRepository : IGenreRepository 
    {
        private readonly AppDbContext _context;

        public GenreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetAllGenreAsync()
        {
            return await _context.Genres.ToListAsync();
        }

        public async Task<Genre?> GetByIdGenreAsync(int id)
        {
            return await _context.Genres.FirstOrDefaultAsync(g => g.GenreId == id);
        }

        public void Add(Genre genre)
        {
            _context.Genres.Add(genre);
            _context.SaveChanges();
        }

        public void Update(Genre genre)
        {
            _context.Genres.Update(genre);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var genre = _context.Genres.FirstOrDefault(g => g.GenreId == id);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
                _context.SaveChanges();
            }
        }
    }
}
