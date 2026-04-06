using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;
using System.Threading.Tasks;

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
                .FirstOrDefaultAsync(u => u.email == request.Email
                                       && u.password == request.Password);

            if (user == null)
            {
                return Unauthorized(new LoginResponse
                {
                    IsAuthenticated = false,
                    Role = null,
                    KlientId = null
                });
            }

            int? klientId = null;
            if (user.role == "klient")
            {
                klientId = user.klients.FirstOrDefault()?.klient_id;
            }

            return Ok(new LoginResponse
            {
                IsAuthenticated = true,
                Role = user.role,
                KlientId = klientId
            });
        }
    }
}