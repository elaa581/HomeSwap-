    namespace HomeSwapAPI.Models
    {
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;  // ← Au lieu de FullName
        public string Email { get; set; } = null!;
        public string? NewPassword { get; set; }
    }
}


