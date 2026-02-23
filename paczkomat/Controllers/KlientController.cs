using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;

namespace paczkomat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly inpostContext _context;

        public ClientsController(inpostContext context)
        {
            _context = context;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<klient>>> GetClients()
        {
            return await _context.klients
                .Include(c => c.packs)
                .Include(c => c.user)
                .ToListAsync();
        }

        // GET: api/clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<klient>> GetClient(int id)
        {
            var klient = await _context.klients
                .Include(c => c.packs)
                .Include(c => c.user)
                .FirstOrDefaultAsync(c => c.klient_id == id);

            if (klient == null)
                return NotFound();

            return klient;
        }

        // POST: api/clients
        [HttpPost]
        public async Task<ActionResult<klient>> CreateClient([FromBody] klient newClient)
        {
            _context.klients.Add(newClient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = newClient.klient_id }, newClient);
        }

        // PUT: api/clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] klient updatedClient)
        {
            if (id != updatedClient.klient_id)
                return BadRequest("ID klienta nie zgadza się z ID w URL");

            _context.Entry(updatedClient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.klients.Any(c => c.klient_id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var klient = await _context.klients.FindAsync(id);
            if (klient == null)
                return NotFound();

            _context.klients.Remove(klient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}