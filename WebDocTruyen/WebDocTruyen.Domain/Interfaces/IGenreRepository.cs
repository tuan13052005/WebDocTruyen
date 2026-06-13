using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Domain.Interfaces
{
    public interface IGenreRepository
    {
        Task<IEnumerable<Genre>> GetAllGenreAsync();
        Task<Genre?> GetByIdGenreAsync(int id);
        void Add(Genre genre);
        void Update(Genre genre);
        void Delete(int id);
    }
}
