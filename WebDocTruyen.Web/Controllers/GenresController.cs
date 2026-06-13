using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebDocTruyen.Application.DTOs.Genre;
using WebDocTruyen.Application.Mappers;
using WebDocTruyen.Application.Services;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class GenresController : Controller
    {
        private readonly IGenreRepository _genreRepo;
        private readonly StoryService _storyService;

        public GenresController(IGenreRepository genreRepo, StoryService storyService)
        {
            _genreRepo = genreRepo;
            _storyService = storyService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var genres = await _genreRepo.GetAllGenreAsync();
            return View(genres.Select(GenreMapper.ToDto).ToList());
        }

        [AllowAnonymous]
        public async Task<IActionResult> ByGenre(int id, int page = 1, int pageSize = 12)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return NotFound();

            var all = await _storyService.GetByGenreAsync(id);
            var stories = all.ToList();
            int total = stories.Count;
            int pages = (int)Math.Ceiling((double)total / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, pages));

            ViewBag.GenreName = genre.Name;
            ViewBag.GenreId = id;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = pages;
            ViewBag.TotalItems = total;
            ViewBag.PageSize = pageSize;
            ViewBag.BaseUrl = Url.Action("ByGenre", new { id }) + $"?pageSize={pageSize}";

            return View(stories.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new GenreDto());

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public IActionResult Create(GenreDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            _genreRepo.Add(new Genre { Name = dto.Name, Description = dto.Description });
            TempData["Success"] = "Thêm thể loại thành công!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return NotFound();
            return View(GenreMapper.ToDto(genre));
        }

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GenreDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var genre = await _genreRepo.GetByIdGenreAsync(dto.GenreId);
            if (genre == null) return NotFound();
            genre.Name = dto.Name;
            genre.Description = dto.Description;
            _genreRepo.Update(genre);
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return NotFound();
            return View(GenreMapper.ToDto(genre));
        }

        [HttpPost, ActionName("Delete"), Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _genreRepo.Delete(id);
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction("Index");
        }
    }
}