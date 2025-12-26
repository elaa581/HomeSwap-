namespace HomeSwapWebClient.Models
{
    public class LoginResponse
    {
        public string token { get; set; } = "";
        public object? user { get; set; }
    }
}
