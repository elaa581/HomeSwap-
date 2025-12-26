using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSwapAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;    // "Electromenager","Meuble",...
        public string Condition { get; set; } = null!; 
        
        // "Neuf","Très bon","Bon","À réparer"
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }


        public string City { get; set; } = null!;
        public string? ImagePath { get; set; }

        // chemin relatif dans wwwroot/uploads

        [NotMapped]
        public IFormFile? Image { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }                  // si tu gères users plus tard
    }
}

