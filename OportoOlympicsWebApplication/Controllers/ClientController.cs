using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using OportoOlympicsWebApplication.Models;

namespace OportoOlympics.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(ApiService apiService, ILogger<ClientController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Client/Login/
        [HttpGet]
        public IActionResult LoginPage()
        {
            return View("Login");
        }

        // GET: Client/Account/
        [HttpGet]
        public IActionResult Account()
        {
            return View("Account");
        }

        // POST: Client/Login/
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var loginData = new { email, password };
            var endpoint = "client/login/";

            _logger.LogInformation("Attempting login with endpoint: {Endpoint}", endpoint);

            try
            {
                var result = await _apiService.PostAsync<dynamic>(endpoint, loginData);

                if (result.Status == "OK" && result.Client != null && result.Client.Count > 0)
                {
                    var client = JsonConvert.DeserializeObject<Client>(JsonConvert.SerializeObject(result.Client[0]));

                    string clientId = client.Id;
                    string clientName = client.Name;
                    string clientEmail = client.Email;

                    HttpContext.Session.SetString("UserId", clientId);
                    HttpContext.Session.SetString("UserName", clientName);
                    HttpContext.Session.SetString("UserEmail", clientEmail);

                    TempData["Message"] = "Login successful!";
                    return RedirectToAction("Dashboard");
                }

                TempData["Error"] = "Invalid login credentials.";
                return RedirectToAction("LoginPage");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Login error: {ex.Message}";
                return RedirectToAction("LoginPage");
            }
        }

        // Método de Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserEmail");
            TempData["Message"] = "You have been logged out.";
            return RedirectToAction("LoginPage");
        }

        // GET: /Client/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: Register Client 
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            var clientData = new { Name = name, Email = email, Password = password };
            var endpoint = "client/";

            try
            {
                await _apiService.PostAsync<object>(endpoint, clientData);

                TempData["Message"] = "Registration successful! You will be redirected to the login page.";

                _logger.LogInformation("Register successful: {email}", email);

                return RedirectToAction("LoginPage");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error registering the client: {ex.Message}";
                return View();
            }
        }

        // GET: /Client/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            return await GetGames();
        }

        // PUT: /Client/UpdatePassword
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(string newPassword)
        {
            var id = HttpContext.Session.GetString("UserId");

            var updateData = new { Password = newPassword };
            var endpoint = $"client/{id}";

            _logger.LogInformation("id: {id}", id);
            _logger.LogInformation("newPassword: {newPassword}", newPassword);
            _logger.LogInformation("endpoint: {endpoint}", endpoint);

            try
            {
                await _apiService.PutAsync<object>(endpoint, updateData);

                TempData["Message"] = "Password successfully updated.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating password: {ex.Message}";
                return View("Error");
            }
        }

        // DELETE: /Client/DeleteAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var id = HttpContext.Session.GetString("UserId");

            var endpoint = $"client/{id}";

            try
            {

                await _apiService.DeleteAsync(endpoint);

                TempData["Message"] = "Account successfully removed.";
                return RedirectToAction("LoginPage");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error removing the account: {ex.Message}";
                return View("Error");
            }
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

                return View("Dashboard");
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