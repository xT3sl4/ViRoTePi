using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;
using System.ComponentModel.DataAnnotations;

namespace paczkomat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly inpostContext _context;

        public UsersController(inpostContext context)
        {
            _context = context;
        }

        public class UserDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public int? PhoneNumber { get; set; }
        }

        public class UserCreateRequest
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            [Required]
            public string Role { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class UserUpdateRequest
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string? Role { get; set; }   
            public string PhoneNumber { get; set; }
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.users.ToListAsync();

            var result = users.Select(u => new UserDto
            {
                Id = u.id,
                Name = u.name ?? "",
                Surname = u.surname ?? "",
                Email = u.email ?? "",
                Role = u.role ?? "",
                PhoneNumber = u.phone_number
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var u = await _context.users.FindAsync(id);
            if (u == null)
                return NotFound(new { message = "Użytkownik nie istnieje." });

            return Ok(new UserDto
            {
                Id = u.id,
                Name = u.name ?? "",
                Surname = u.surname ?? "",
                Email = u.email ?? "",
                Role = u.role ?? "",
                PhoneNumber = u.phone_number
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Imię jest wymagane." });
            if (string.IsNullOrWhiteSpace(request.Surname))
                return BadRequest(new { message = "Nazwisko jest wymagane." });
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email jest wymagany." });
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Hasło jest wymagane." });
            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new { message = "Rola jest wymagana." });

            var emailExists = await _context.users.AnyAsync(u => u.email == request.Email);
            if (emailExists)
                return BadRequest(new { message = "Użytkownik z tym adresem email już istnieje." });

            int? phone = null;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                string cleanPhone = request.PhoneNumber.Replace(" ", "").Replace("-", "");
                if (!int.TryParse(cleanPhone, out int parsedPhone))
                    return BadRequest(new { message = "Nieprawidłowy format numeru telefonu." });
                phone = parsedPhone;

                var phoneExists = await _context.users.AnyAsync(u => u.phone_number == parsedPhone);
                if (phoneExists)
                    return BadRequest(new { message = "Użytkownik z tym numerem telefonu już istnieje." });
            }

            var newUser = new user
            {
                name = request.Name.Trim(),
                surname = request.Surname.Trim(),
                email = request.Email.Trim(),
                password = request.Password,
                role = request.Role,
                phone_number = phone
            };

            _context.users.Add(newUser);
            await _context.SaveChangesAsync();

            if (request.Role == "klient")
            {
                _context.klients.Add(new klient { user_id = newUser.id, pack_id = 0 });
                await _context.SaveChangesAsync();
            }
            else if (request.Role == "kurier")
            {
                _context.kuriers.Add(new kurier { user_id = newUser.id, state = "available" });
                await _context.SaveChangesAsync();
            }
            else if (request.Role == "admin")
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO admins (user_id) VALUES ({0})", newUser.id);
            }

            return Ok(new { message = "Użytkownik został utworzony.", userId = newUser.id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Użytkownik nie istnieje." });

            
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.name = request.Name.Trim();

            
            if (!string.IsNullOrWhiteSpace(request.Surname))
                user.surname = request.Surname.Trim();

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.email)
            {
                var emailExists = await _context.users.AnyAsync(u => u.email == request.Email && u.id != id);
                if (emailExists)
                    return BadRequest(new { message = "Ten adres email jest już zajęty." });
                user.email = request.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.password = request.Password;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                string cleanPhone = request.PhoneNumber.Replace(" ", "").Replace("-", "");
                if (!int.TryParse(cleanPhone, out int parsedPhone))
                    return BadRequest(new { message = "Nieprawidłowy format numeru telefonu." });

                var phoneExists = await _context.users.AnyAsync(u => u.phone_number == parsedPhone && u.id != id);
                if (phoneExists)
                    return BadRequest(new { message = "Ten numer telefonu jest już zajęty." });

                user.phone_number = parsedPhone;
            }

            if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != user.role)
            {
                if (user.role == "klient")
                {
                    var klient = await _context.klients.FirstOrDefaultAsync(k => k.user_id == id);
                    if (klient != null) _context.klients.Remove(klient);
                }
                else if (user.role == "kurier")
                {
                    var kurier = await _context.kuriers.FirstOrDefaultAsync(k => k.user_id == id);
                    if (kurier != null) _context.kuriers.Remove(kurier);
                }
                else if (user.role == "admin")
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM admins WHERE user_id = {0}", id);
                }

                user.role = request.Role;
                await _context.SaveChangesAsync();

                if (request.Role == "klient")
                {
                    _context.klients.Add(new klient { user_id = id, pack_id = 0 });
                }
                else if (request.Role == "kurier")
                {
                    _context.kuriers.Add(new kurier { user_id = id, state = "available" });
                }
                else if (request.Role == "admin")
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO admins (user_id) VALUES ({0})", id);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Użytkownik został zaktualizowany." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Użytkownik nie istnieje." });

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Użytkownik został usunięty." });
        }
    }
}