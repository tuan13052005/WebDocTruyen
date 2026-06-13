using WebDocTruyen.Application.DTOs.Story;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Application.Services
{
    public class StoryService
    {
        private readonly IStoryRepository _storyRepo;

        public StoryService(IStoryRepository storyRepo)
        {
            _storyRepo = storyRepo;
        }

        public async Task<IEnumerable<StoryDto>> GetAllStoriesAsync()
        {
            var stories = await _storyRepo.GetAllAsync();
            return stories.Select(StoryMapper.ToDto);
        }

        public async Task<StoryDto?> GetByIdAsync(int id)
        {
            var s = await _storyRepo.GetByIdAsync(id);
            return s == null ? null : StoryMapper.ToDto(s);
        }

        public async Task<IEnumerable<StoryDto>> SearchAsync(string keyword)
        {
            var stories = await _storyRepo.SearchAsync(keyword);
            return stories.Select(StoryMapper.ToDto);
        }

        public async Task<IEnumerable<StoryDto>> GetByGenreAsync(int genreId)
        {
            var stories = _storyRepo.GetByGenre(genreId);
            return stories.Select(StoryMapper.ToDto);
        }
    }
}