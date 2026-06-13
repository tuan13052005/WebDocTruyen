using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDocTruyen.Domain.Entities;

namespace WebDocTruyen.Domain.Interfaces
{
    public interface IStoryRepository
    {
        Task<Story?> GetByIdAsync(int id);
        Task<IEnumerable<Story>> GetAllAsync();
        Task AddAsync(Story story);
        Task UpdateAsync(Story story);
        Task DeleteAsync(int id);
        Task<IEnumerable<Story>> SearchAsync(string keyword);
        IEnumerable<Story> GetByGenre(int genreId);
    }
}
