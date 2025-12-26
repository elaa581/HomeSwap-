using HomeSwapWebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HomeSwapWebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public AccountController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public IActionResult Register() => View();
        public IActionResult Login() => View();
        

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 🔹 Affichage du ModelState et des valeurs pour debug
            Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);
            Console.WriteLine("FullName: " + model.FullName);
            Console.WriteLine("Email: " + model.Email);
            Console.WriteLine("Password: " + model.Password);

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpFactory.CreateClient("api");

            // 🔹 Envoi des données à l'API
            var res = await client.PostAsJsonAsync("api/Auth/register", model);

            // 🔹 Lecture de la réponse brute pour debug
            var debugResponse = await res.Content.ReadAsStringAsync();
            Console.WriteLine("API Register Response: " + debugResponse);

            // 🔹 Si l'API retourne une erreur
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Erreur lors de l'inscription : " + debugResponse);
                return View(model);
            }

            // 🔹 Si tout est OK, afficher un message ou rediriger vers Login
            TempData["Message"] = "Inscription réussie, connectez-vous.";
            return RedirectToAction("Login");
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);
            Console.WriteLine("Email: " + model.Email);
            Console.WriteLine("Password: " + model.Password);

            var client = _httpFactory.CreateClient("api");
            var res = await client.PostAsJsonAsync("api/Auth/login", model);
            var debugResponse = await res.Content.ReadAsStringAsync();
            Console.WriteLine("API Response: " + debugResponse);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Email ou mot de passe invalide");
                return View(model);
            }

            var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
            HttpContext.Session.SetString("JWToken", payload!.token);

            return RedirectToAction("Index", "ProductClient");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            return RedirectToAction("Index", "ProductClient");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var client = _httpFactory.CreateClient("api");
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync("api/Auth/me"); // ✅ Changé ici !

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                ViewBag.ErrorDetails = $"Status: {res.StatusCode}, Error: {error}";
                return View("Error");
            }

            var user = await res.Content.ReadFromJsonAsync<ProfileViewModel>();
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpFactory.CreateClient("api");
            var token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.PutAsJsonAsync("api/Auth/update-profile", model); // ✅ Changé ! // À créer dans l’API

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Erreur lors de la mise à jour : " + error);
                return View(model);
            }

            TempData["Message"] = "Profil mis à jour avec succès.";
            return RedirectToAction("Profile");
        }

    }
}


