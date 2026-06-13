using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Domain.Entities;
using WebDocTruyen.Domain.Interfaces;

namespace WebDocTruyen.Web.Controllers
{
    public class GenresController : Controller
    {
        private readonly IStoryRepository _storyRepo;
        private readonly IGenreRepository _genreRepo;

        public GenresController(IGenreRepository genreRepo, IStoryRepository storyRepo)
        {
            _genreRepo = genreRepo;
            _storyRepo = storyRepo;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var genres = await _genreRepo.GetAllGenreAsync();
            return View(genres);
        }

        // Thêm phân trang cho ByGenre
        [AllowAnonymous]
        public async Task<IActionResult> ByGenre(int id, int page = 1, int pageSize = 12)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return NotFound();

            var stories = _storyRepo.GetByGenre(id).ToList();
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
        public IActionResult Create() => View();

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public IActionResult Create(Genre genre)
        {
            if (!ModelState.IsValid) return View(genre);
            _genreRepo.Add(genre);
            TempData["Success"] = "Thêm thể loại thành công!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return NotFound();
            return View(genre);
        }

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public IActionResult Edit(Genre genre)
        {
            if (!ModelState.IsValid) return View(genre);
            _genreRepo.Update(genre);
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _genreRepo.GetByIdGenreAsync(id);
            if (genre == null) return NotFound();
            return View(genre);
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