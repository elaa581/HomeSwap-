using Microsoft.EntityFrameworkCore;
using HomeSwapAPI.Models;

namespace HomeSwapAPI.Data
{
    public class HomeSwapDbContext : DbContext
    {
        public HomeSwapDbContext(DbContextOptions<HomeSwapDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // seed data (d'exemple)
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Title = "Machine à laver Samsung 7kg",
                    Description = "Bon état, peu utilisée",
                    Category = "Électroménager",
                    Condition = "Bon",
                    Price = 420,
                    City = "Sousse",
                    ImagePath = null,
                    DateCreated = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Title = "Canapé 3 places",
                    Description = "Tissu beige, très confortable",
                    Category = "Meuble",
                    Condition = "Très bon",
                    Price = 350,
                    City = "Monastir",
                    ImagePath = null,
                    DateCreated = DateTime.UtcNow
                }
            );
        }
    }
}

