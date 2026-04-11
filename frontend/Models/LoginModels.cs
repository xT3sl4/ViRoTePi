namespace frontend.Models
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Role { get; set; }
    }
}