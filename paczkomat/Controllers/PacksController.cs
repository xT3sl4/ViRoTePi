using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;

namespace paczkomat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacksController : ControllerBase
    {
        private readonly inpostContext _context;

        public PacksController(inpostContext context)
        {
            _context = context;
        }

        // GET: api/packs?size=M&delivered=false&sort=date_desc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<pack>>> GetPacks(
            [FromQuery] string? size,
            [FromQuery] bool? delivered,
            [FromQuery] int? klientId,
            [FromQuery] string? sort)
        {
            var query = _context.packs
                .Include(p => p.klient)
                .AsQueryable();

            // Filtrowanie
            if (!string.IsNullOrEmpty(size))
                query = query.Where(p => p.size == size);
            if (delivered.HasValue)
                query = query.Where(p => p.delivered == delivered);
            if (klientId.HasValue)
                query = query.Where(p => p.klient_id == klientId);

            // Sortowanie
            query = sort switch
            {
                "date_asc" => query.OrderBy(p => p.date),
                "date_desc" => query.OrderByDescending(p => p.date),
                "size_asc" => query.OrderBy(p => p.size),
                "size_desc" => query.OrderByDescending(p => p.size),
                _ => query.OrderBy(p => p.pack_id)
            };

            return await query.ToListAsync();
        }

        // GET: api/packs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<pack>> GetPack(int id)
        {
            var pack = await _context.packs
                .Include(p => p.klient)
                .FirstOrDefaultAsync(p => p.pack_id == id);

            if (pack == null)
                return NotFound();

            return pack;
        }

        // POST: api/packs
        [HttpPost]
        public async Task<ActionResult<pack>> CreatePack([FromBody] pack newPack)
        {
            if (newPack.klient_id.HasValue)
            {
                var klient = await _context.klients.FindAsync(newPack.klient_id);
                if (klient == null)
                    return BadRequest($"Klient o id {newPack.klient_id} nie istnieje.");

                newPack.klient = klient;
            }

            _context.packs.Add(newPack);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPack), new { id = newPack.pack_id }, newPack);
        }

        // PUT: api/packs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePack(int id, [FromBody] pack updatedPack)
        {
            if (id != updatedPack.pack_id)
                return BadRequest("ID paczki nie zgadza się z ID w URL");

            _context.Entry(updatedPack).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.packs.Any(p => p.pack_id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/packs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePack(int id)
        {
            var pack = await _context.packs.FindAsync(id);
            if (pack == null)
                return NotFound();

            _context.packs.Remove(pack);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}