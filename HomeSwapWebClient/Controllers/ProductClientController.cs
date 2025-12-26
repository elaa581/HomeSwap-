using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using HomeSwapWebClient.Models;
using System.Net.Http.Headers;

namespace HomeSwapWebClient.Controllers
{
    public class ProductClientController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public ProductClientController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        // 🔥 méthode correcte pour récupérer un HttpClient + token
        private HttpClient CreateApiClient()
        {
            var client = _httpFactory.CreateClient("api");

            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // INDEX
        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();

            var response = await client.GetAsync("api/Products/get-all-products");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ApiError = await response.Content.ReadAsStringAsync();
                return View(new List<ProductClient>());
            }

            var items = await response.Content.ReadFromJsonAsync<List<ProductClient>>();

            return View(items ?? new List<ProductClient>());
        }


        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateApiClient();

            var item = await client.GetFromJsonAsync<ProductClient>(
                $"api/Products/get-product-by-id/{id}"
            );

            if (item == null) return NotFound();

            return View(item);
        }

        // CREATE
        public IActionResult Create() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductClient product, IFormFile Image)
        {
            var client = CreateApiClient();

            if (!ModelState.IsValid)
                return View(product);

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(product.Title ?? ""), "Title");
            form.Add(new StringContent(product.Description ?? ""), "Description");
            form.Add(new StringContent(product.Category ?? ""), "Category");
            form.Add(new StringContent(product.Condition ?? ""), "Condition");
            form.Add(new StringContent(product.Price.ToString()), "Price");
            form.Add(new StringContent(product.City ?? ""), "City");

            if (Image != null && Image.Length > 0)
            {
                var fileContent = new StreamContent(Image.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(Image.ContentType);
                form.Add(fileContent, "Image", Image.FileName); // ⚡ Le nom "Image" doit matcher ProductCreateDto.Image
            }

            var response = await client.PostAsync("api/Products/create-product", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Erreur lors de l’ajout : " + error);
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }




        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateApiClient();

            var product = await client.GetFromJsonAsync<ProductClient>(
                $"api/Products/get-product-by-id/{id}"
            );

            if (product == null) return NotFound();

            return View(product);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductClient product)
        {
            if (id != product.Id)
                return BadRequest();

            var client = CreateApiClient();

            var res = await client.PutAsJsonAsync(
                $"api/Products/edit-product/{id}",
                product
            );

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Échec de la modification");
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateApiClient();

            var product = await client.GetFromJsonAsync<ProductClient>(
                $"api/Products/get-product-by-id/{id}"
            );

            if (product == null) return NotFound();

            return View(product);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = CreateApiClient();

            var res = await client.DeleteAsync($"api/Products/delete-product/{id}");

            return RedirectToAction(nameof(Index));
        }
    }
}



