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
                KlientId = klientId,
                KurierId = kurierId
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
                KlientId = klientId,
                KurierId = kurierId
            });
        }
    }
}