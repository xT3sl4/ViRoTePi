using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;
using System.Xml.Serialization;

namespace paczkomat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacksController : ControllerBase
    {
        private readonly inpostContext _context;

        public PacksController(inpostContext context)
        {
            _context = context;
        }

        // GET: api/packs?delivered=false&klientId=5&sortBy=date&sortDesc=true
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool? delivered, [FromQuery] int? klientId,
                                             [FromQuery] string? sortBy, [FromQuery] bool sortDesc = false)
        {
            IQueryable<pack> query = _context.packs.Include(p => p.klient)
                                                   .Include(p => p.boxes);

            if (delivered.HasValue)
                query = query.Where(p => p.delivered == delivered);

            if (klientId.HasValue)
                query = query.Where(p => p.klient_id == klientId);

            // sortowanie
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = (sortBy.ToLower(), sortDesc) switch
                {
                    ("date", false) => query.OrderBy(p => p.date),
                    ("date", true) => query.OrderByDescending(p => p.date),
                    ("to_when", false) => query.OrderBy(p => p.to_when),
                    ("to_when", true) => query.OrderByDescending(p => p.to_when),
                    ("size", false) => query.OrderBy(p => p.size),
                    ("size", true) => query.OrderByDescending(p => p.size),
                    _ => query
                };
            }

            var packs = await query.ToListAsync();
            return Ok(packs);
        }

        // GET: api/packs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pack = await _context.packs.Include(p => p.klient)
                                           .Include(p => p.boxes)
                                           .FirstOrDefaultAsync(p => p.pack_id == id);
            if (pack == null) return NotFound();
            return Ok(pack);
        }

        // POST: api/packs
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] pack pack)
        {
            if (pack == null) return BadRequest();

            _context.packs.Add(pack);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = pack.pack_id }, pack);
        }

        // PUT: api/packs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] pack pack)
        {
            if (id != pack.pack_id) return BadRequest("ID nie pasuje do paczki.");

            var existingPack = await _context.packs.FindAsync(id);
            if (existingPack == null) return NotFound();

            // aktualizacja pól
            existingPack.size = pack.size;
            existingPack.klient_id = pack.klient_id;
            existingPack.delivered = pack.delivered;
            existingPack.date = pack.date;
            existingPack.to_when = pack.to_when;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/packs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pack = await _context.packs.FindAsync(id);
            if (pack == null) return NotFound();

            _context.packs.Remove(pack);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}