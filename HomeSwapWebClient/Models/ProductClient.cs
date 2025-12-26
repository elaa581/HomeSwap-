namespace HomeSwapWebClient.Models
    
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;



    public class ProductClient
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Category { get; set; } = null!;
            public string Condition { get; set; } = null!;
            public string City { get; set; } = null!;
            public string? ImagePath { get; set; }
            public DateTime DateCreated { get; set; }
            public int UserId { get; set; }
            public decimal Price { get; set; }

        [JsonIgnore]
        [NotMapped]
        public IFormFile? Image { get; set; }

    }
}