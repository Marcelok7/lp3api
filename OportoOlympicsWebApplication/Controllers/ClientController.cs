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

        // GET: /Client/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
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
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Login error: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        // GET: /Client/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Client/Register
        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            var clientData = new { Name = name, Email = email, Password = password };
            var endpoint = "client";

            try
            {
                await _apiService.PostAsync<object>(endpoint, clientData);

                TempData["Message"] = "Registration successful. Check your email for the password.";
                return RedirectToAction("Login");
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
            var userName    = HttpContext.Session.GetString("UserName");
            var userId      = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userName))
            {
                TempData["Error"] = "You must be logged in to access the Dashboard.";
                return RedirectToAction("LoginPage");
            }

            try
            {
                var endpoint = "client";
                var result = await _apiService.GetAsync<dynamic>(endpoint);

                if (result.Status == "OK" && result.Clients != null)
                {
                    var clients = JsonConvert.DeserializeObject<List<Client>>(JsonConvert.SerializeObject(result.Clients));

                    ViewData["UserName"]    = userName;
                    ViewData["UserId"]      = userId;

                    return View(clients);
                }

                TempData["Error"] = "Failed to fetch clients.";
                ViewData["UserName"] = userName;
                return View(new List<Client>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading dashboard: {ex.Message}";
                ViewData["UserName"] = userName;
                return View(new List<Client>());
            }
        }


        // GET: /Client/All
        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var endpoint = "client";

            try
            {
                var clients = await _apiService.GetAsync<List<object>>(endpoint);

                return View("ClientList", clients);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fetching clients: {ex.Message}";
                return View("Error");
            }
        }

        // POST: /Client/Ban
        [HttpPost]
        public async Task<IActionResult> BanClient(int id)
        {
            var updateData = new { Active = false };
            var endpoint = $"client/{id}";

            try
            {
                await _apiService.PutAsync<object>(endpoint, updateData);

                TempData["Message"] = "Client successfully banned.";
                return RedirectToAction("GetAllClients");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error banning the client: {ex.Message}";
                return View("Error");
            }
        }

        // POST: /Client/UpdatePassword
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(int id, string newPassword)
        {
            var updateData = new { Password = newPassword };
            var endpoint = $"client/{id}";

            _logger.LogInformation("id: " , id);
            _logger.LogInformation("newPassword: ", newPassword);

            _logger.LogInformation("endpoint: ", endpoint);

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

        // POST: /Client/Delete
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var endpoint = $"client/{id}";

            try { 
            
                await _apiService.DeleteAsync(endpoint);

                TempData["Message"] = "Account successfully removed.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error removing the account: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "You have successfully logged out.";
            return RedirectToAction("LoginPage");
        }
    }
}