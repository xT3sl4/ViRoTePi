namespace paczkomat.Models
{
    public class LoginResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Role { get; set; }
        public int? UserId { get; set; }
        public int? KlientId { get; set; }
        public int? KurierId { get; set; }
        public string Selfie { get; set; }
    }
}
