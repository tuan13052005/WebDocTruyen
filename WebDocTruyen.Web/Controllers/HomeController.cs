using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
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
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, StoryService storyService,
            IGenreRepository genreRepo, IWebHostEnvironment env)
        {
            _logger = logger;
            _storyService = storyService;
            _genreRepo = genreRepo;
            _env = env;
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

        // ── Xử lý exception chưa bắt (đăng ký trong Program.cs: app.UseExceptionHandler("/Home/Error")) ──
        [AllowAnonymous]
        [Route("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionFeature?.Error;
            var path = exceptionFeature?.Path;

            if (exception != null)
                _logger.LogError(exception, "Lỗi chưa xử lý tại {Path} | RequestId: {RequestId}", path, requestId);

            var model = ErrorViewModel.FromStatusCode(500, requestId, path);
            model.ShowDetails = _env.IsDevelopment();
            if (model.ShowDetails && exception != null)
            {
                model.ExceptionMessage = exception.Message;
                model.StackTrace = exception.StackTrace;
            }

            Response.StatusCode = 500;
            return View(model);
        }

        // ── Xử lý mã lỗi HTTP (404, 403...) — đăng ký: app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}") ──
        [AllowAnonymous]
        [Route("Home/StatusCode/{code:int}")]
        public IActionResult StatusCodeError(int code)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var path = statusFeature?.OriginalPath;

            var model = ErrorViewModel.FromStatusCode(code, requestId, path);
            Response.StatusCode = code;
            return View("Error", model);
        }
    }
}