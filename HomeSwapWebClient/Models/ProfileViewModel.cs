namespace HomeSwapWebClient.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? NewPassword { get; set; } // Optionnel pour changer mot de passe
    }

}
