using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OportoOlympicsWebApplication.Models;

namespace OportoOlympics.Controllers
{
    public class GameController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<GameController> _logger;

        public GameController(ApiService apiService, ILogger<GameController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET:
        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "You must be logged in to access the Dashboard.";
                return RedirectToAction("LoginPage", "Client");
            }

            try
            {
                var endpoint = "game/";

                var result = await _apiService.GetAsync<GameResponse>(endpoint);

                List<Game> games = new List<Game>();

                if (result.Status == "OK" && result.Games != null)
                {
                    games = result.Games;
                }

                ViewData["UserName"] = userName;
                ViewData["UserId"] = userId;
                ViewData["Games"] = games;

                return View("Client/Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading dashboard: {ex.Message}";
                ViewData["UserName"] = userName;
                return View("Error");
            }
        }
    }
}