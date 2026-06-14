using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebDocTruyen.Application.Mapper;
using WebDocTruyen.Application.Services;
using WebDocTruyen.Domain.Interfaces;
using WebDocTruyen.Web.Models;

namespace WebDocTruyen.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly StoryService _storyService;
        private readonly IGenreRepository _genreRepo;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, StoryService storyService, IGenreRepository genreRepo)
        {
            _logger = logger;
            _storyService = storyService;
            _genreRepo = genreRepo;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var stories = await _storyService.GetAllStoriesAsync();
            ViewBag.Genres = (await _genreRepo.GetAllGenreAsync()).Select(GenreMapper.ToDto).ToList();
            return View(stories.OrderByDescending(s => s.CreatedAt).Take(8).ToList());
        }

        [AllowAnonymous]
        public IActionResult Privacy() => View();

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}