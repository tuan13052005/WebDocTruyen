using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebDocTruyen.Application.Services;
using WebDocTruyen.Web.Models;

namespace WebDocTruyen.Web.Controllers;

public class HomeController : Controller
{
    private readonly StoryService _storyService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, StoryService storyService)
    {
        _logger = logger;
        _storyService = storyService;
    }

    // FIX: ??i IndexAsync ? Index (ASP.NET routing không nh?n IndexAsync)
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var stories = await _storyService.GetAllStoriesAsync();
        return View(stories.Take(8));
    }

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}