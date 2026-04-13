using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;

namespace paczkomat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly inpostContext _context;

        public AuthController(inpostContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.users
                .Include(u => u.klients)
                .Include(u => u.kuriers)
                .FirstOrDefaultAsync(u => u.email == request.Email
                                       && u.password == request.Password);

            if (user == null)
                return Unauthorized(new LoginResponse { IsAuthenticated = false });

            int? klientId = null;
            int? kurierId = null;

            if (user.role == "klient")
                klientId = user.klients.FirstOrDefault()?.klient_id;

            if (user.role == "kurier")
            {
                var kurier = await _context.kuriers.FirstOrDefaultAsync(k => k.user_id == user.id);
                kurierId = kurier?.kurier_id;
            }

            return Ok(new LoginResponse
            {
                IsAuthenticated = true,
                Role = user.role,
                UserId = user.id,
                KlientId = klientId,
                KurierId = kurierId,
                Selfie = user.selfie
            });
        }

        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Email))
                return BadRequest("Email jest wymagany.");

            var user = await _context.users
                .FirstOrDefaultAsync(u => u.email == request.Email);

            if (user == null)
                return Unauthorized(new LoginResponse { IsAuthenticated = false });

            int? klientId = null;
            int? kurierId = null;

            if (user.role == "klient")
            {
                var klient = await _context.klients.FirstOrDefaultAsync(k => k.user_id == user.id);
                klientId = klient?.klient_id;
            }

            if (user.role == "kurier")
            {
                var kurier = await _context.kuriers.FirstOrDefaultAsync(k => k.user_id == user.id);
                kurierId = kurier?.kurier_id;
            }

            return Ok(new LoginResponse
            {
                IsAuthenticated = true,
                Role = user.role,
                UserId = user.id,
                KlientId = klientId,
                KurierId = kurierId,
                Selfie = user.selfie
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email jest wymagany." });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Hasło jest wymagane." });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Imię jest wymagane." });

            if (string.IsNullOrWhiteSpace(request.Surname))
                return BadRequest(new { message = "Nazwisko jest wymagane." });

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest(new { message = "Numer telefonu jest wymagany." });

            var existingUserByEmail = await _context.users
                .FirstOrDefaultAsync(u => u.email == request.Email);

            if (existingUserByEmail != null)
                return BadRequest(new { message = "Użytkownik z tym adresem email już istnieje." });

            int phoneNumber;
            if (!int.TryParse(request.PhoneNumber.Replace(" ", ""), out phoneNumber))
                return BadRequest(new { message = "Nieprawidłowy format numeru telefonu." });

            var existingUserByPhone = await _context.users
                .FirstOrDefaultAsync(u => u.phone_number == phoneNumber);

            if (existingUserByPhone != null)
                return BadRequest(new { message = "Użytkownik z tym numerem telefonu już istnieje." });

            var newUser = new user
            {
                name = request.Name,
                surname = request.Surname,
                email = request.Email,
                password = request.Password,
                phone_number = phoneNumber,
                role = "klient"
            };

            _context.users.Add(newUser);
            await _context.SaveChangesAsync();

            var newKlient = new klient
            {
                user_id = newUser.id,
                pack_id = 0
            };

            _context.klients.Add(newKlient);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Konto zostało utworzone pomyślnie.",
                userId = newUser.id,
                klientId = newKlient.klient_id
            });
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email jest wymagany." });

            var user = await _context.users
                .FirstOrDefaultAsync(u => u.email == email);

            return Ok(new { exists = user != null });
        }

        [HttpGet("check-phone")]
        public async Task<IActionResult> CheckPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(new { message = "Numer telefonu jest wymagany." });

            int phoneNumber;
            if (!int.TryParse(phone.Replace(" ", ""), out phoneNumber))
                return BadRequest(new { message = "Nieprawidłowy format numeru telefonu." });

            var user = await _context.users
                .FirstOrDefaultAsync(u => u.phone_number == phoneNumber);

            return Ok(new { exists = user != null });
        }
    }
}