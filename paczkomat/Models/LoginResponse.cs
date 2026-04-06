namespace paczkomat.Models
{
    public class LoginResponse
    {
        public bool IsAuthenticated { get; set; }
        public string? Role { get; set; }
        public int? KlientId { get; set; } 
    }
}