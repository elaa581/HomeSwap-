using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeSwapAPI.Data;
using HomeSwapAPI.Models;
using Microsoft.AspNetCore.Authorization;


namespace HomeSwapAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly HomeSwapDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(HomeSwapDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet("get-all-products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll([FromQuery] string? category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var q = _db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category)) q = q.Where(p => p.Category == category);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            var items = await q.OrderByDescending(p => p.DateCreated)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
            return Ok(items);
        }

        [HttpGet("get-product-by-id/{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // Create with image upload (multipart/form-data)
        [HttpPost("create-product")]
        [Authorize]
        public async Task<ActionResult<Product>> Create([FromForm] ProductCreateDto dto)
        {
            string? imagePath = null;

            // Enregistrer le fichier si envoyé
            if (dto.Image != null)
            {
                imagePath = await SaveFile(dto.Image);
            }

            var p = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Condition = dto.Condition,
                Price = dto.Price,
                City = dto.City,
                ImagePath = imagePath,
                DateCreated = DateTime.UtcNow
            };

            _db.Products.Add(p);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
        }


        [HttpPut("edit-product/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] Product edit)
        {
            if (id != edit.Id) return BadRequest();
            var existing = await _db.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = edit.Title;
            existing.Description = edit.Description;
            existing.Category = edit.Category;
            existing.Condition = edit.Condition;
            existing.Price = edit.Price;
            existing.City = edit.City;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("delete-product/{id}")]
              
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploads, fileName);
            using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            // ✅ CORRIGÉ : retourne seulement le nom du fichier
            return fileName;
        }



    }

    // DTO pour Create (upload)
    public class ProductCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Condition { get; set; } = null!;
        public decimal Price { get; set; }
        public string City { get; set; } = null!;
        public IFormFile? Image { get; set; }
    }
}

