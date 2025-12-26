using HomeSwapAPI.Data;
using HomeSwapAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HomeSwapAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HomeSwapDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(HomeSwapDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email déjà utilisé." });

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Inscription réussie." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized(new { message = "Email ou mot de passe invalide." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Email ou mot de passe invalide." });

            // Générer JWT
            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Id, user.FullName, user.Email } });
        }

        private string GenerateJwtToken(User user)
        {
            // Lecture via indexeur "Jwt:Key" ou GetValue
            var keyValue = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var durationStr = _config["Jwt:DurationInMinutes"];

            if (string.IsNullOrWhiteSpace(keyValue))
                throw new InvalidOperationException("Configuration manquante : Jwt:Key est vide. Vérifie appsettings.json et que le fichier est copié dans le dossier de sortie.");
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(durationStr))
                throw new InvalidOperationException("Configuration JWT incomplète (Issuer/Audience/DurationInMinutes).");

            if (!double.TryParse(durationStr, out var durationMinutes))
                throw new InvalidOperationException("Jwt:DurationInMinutes doit être un nombre (ex: \"60\").");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("fullname", user.FullName ?? string.Empty)
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(durationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ProfileViewModel>> Me()
        {
            // Debug : affiche tous les claims
            Console.WriteLine("=== CLAIMS DEBUG ===");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
            Console.WriteLine("===================");

            // ✅ Essaie TOUS les formats possibles du claim Sub
            var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            Console.WriteLine($"Sub claim value: {subClaim}");

            if (string.IsNullOrEmpty(subClaim))
                return Unauthorized("Token invalide ou claim manquant");

            if (!int.TryParse(subClaim, out var userId))
                return Unauthorized("ID utilisateur invalide");

            Console.WriteLine($"Searching for user ID: {userId}");

            var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                Console.WriteLine($"USER NOT FOUND with ID: {userId}");
                return NotFound($"Utilisateur {userId} introuvable");
            }

            Console.WriteLine($"User found: {user.FullName}");

            return new ProfileViewModel
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email
            };
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = model.Name;  // ← Changé ici (était model.Name)
            user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }




        // DTOs
        public record RegisterDto(string FullName, string Email, string Password);
        public record LoginDto(string Email, string Password);
    }
}

