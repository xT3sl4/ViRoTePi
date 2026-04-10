using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using paczkomat.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public class PackDto
        {
            public int PackId { get; set; }
            public string? Size { get; set; }
            public string Delivered { get; set; } = "";
            public string? Date { get; set; }
            public string KlientName { get; set; } = "";
        }

        [HttpGet]
        public async Task<IEnumerable<PackDto>> GetPacks(
            [FromQuery] string? size,
            [FromQuery] bool? delivered,
            [FromQuery] int? klientId,
            [FromQuery] int? userId,
            [FromQuery] string? sort)
        {
            if (!klientId.HasValue && userId.HasValue)
            {
                var klient = await _context.klients.FirstOrDefaultAsync(k => k.user_id == userId.Value);
                if (klient != null)
                    klientId = klient.klient_id;
            }

            var query = _context.packs.AsQueryable();

            if (!string.IsNullOrEmpty(size))
                query = query.Where(p => p.size == size);
            if (delivered.HasValue)
                query = query.Where(p => p.delivered == delivered);
            if (klientId.HasValue)
                query = query.Where(p => p.klient_id == klientId);

            query = sort switch
            {
                "date_asc" => query.OrderBy(p => p.date),
                "date_desc" => query.OrderByDescending(p => p.date),
                "size_asc" => query.OrderBy(p => p.size),
                "size_desc" => query.OrderByDescending(p => p.size),
                _ => query.OrderBy(p => p.pack_id)
            };

            var list = await query.ToListAsync();
            var packDtos = new List<PackDto>();

            foreach (var pack in list)
            {
                var paczkomatAddress = await (from pd in _context.paczkomat_data
                                              join pac in _context.paczkomats on pd.paczkomat_id equals pac.paczkomat_id
                                              join b in _context.boxs on pd.box_id equals b.box_id
                                              where b.pack_id == pack.pack_id
                                              select pac.address)
                                             .FirstOrDefaultAsync();

                packDtos.Add(new PackDto
                {
                    PackId = pack.pack_id,
                    Size = pack.size,
                    Delivered = pack.delivered.HasValue
                        ? (pack.delivered.Value ? "Doręczona" : "W drodze")
                        : "Nieznany",
                    Date = pack.date?.ToString("yyyy-MM-dd"),
                    KlientName = paczkomatAddress ?? "Brak paczkomatu"
                });
            }

            return packDtos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PackDto>> GetPack(int id)
        {
            var pack = await _context.packs
                .Include(p => p.boxes)
                .FirstOrDefaultAsync(p => p.pack_id == id);

            if (pack == null)
                return NotFound();

            var paczkomatAddress = await (from pd in _context.paczkomat_data
                                          join pac in _context.paczkomats on pd.paczkomat_id equals pac.paczkomat_id
                                          join b in _context.boxs on pd.box_id equals b.box_id
                                          where b.pack_id == id
                                          select pac.address)
                                         .FirstOrDefaultAsync();

            return new PackDto
            {
                PackId = pack.pack_id,
                Size = pack.size,
                Delivered = pack.delivered.HasValue
                    ? (pack.delivered.Value ? "Doręczona" : "W drodze")
                    : "Nieznany",
                Date = pack.date?.ToString("yyyy-MM-dd"),
                KlientName = paczkomatAddress ?? "Brak paczkomatu"
            };
        }

        [HttpGet("kurier/{kurierId}")]
        public async Task<IEnumerable<PackDto>> GetPacksByKurier(int kurierId, [FromQuery] bool? delivered)
        {
            var packIds = await _context.kuriers_data
                .Where(kd => kd.kurier_id == kurierId)
                .Select(kd => kd.pack_id)
                .ToListAsync();

            var query = _context.packs
                .Where(p => packIds.Contains(p.pack_id))
                .AsQueryable();

            if (delivered.HasValue)
                query = query.Where(p => p.delivered == delivered);

            var packs = await query.ToListAsync();
            var packDtos = new List<PackDto>();

            foreach (var pack in packs)
            {
                var paczkomatAddress = await (from pd in _context.paczkomat_data
                                              join pac in _context.paczkomats on pd.paczkomat_id equals pac.paczkomat_id
                                              join b in _context.boxs on pd.box_id equals b.box_id
                                              where b.pack_id == pack.pack_id
                                              select pac.address)
                                             .FirstOrDefaultAsync();

                packDtos.Add(new PackDto
                {
                    PackId = pack.pack_id,
                    Size = pack.size,
                    Delivered = pack.delivered.HasValue
                        ? (pack.delivered.Value ? "Doręczona" : "W drodze")
                        : "Nieznany",
                    Date = pack.date?.ToString("yyyy-MM-dd"),
                    KlientName = paczkomatAddress ?? "Brak paczkomatu"
                });
            }

            return packDtos;
        }

        [HttpPut("{id}/deliver")]
        public async Task<IActionResult> DeliverPack(int id)
        {
            var pack = await _context.packs.FindAsync(id);
            if (pack == null)
                return NotFound();

            pack.delivered = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<pack>> CreatePack([FromBody] pack newPack)
        {
            if (newPack.klient_id.HasValue)
            {
                var klient = await _context.klients
                    .Include(k => k.user)
                    .FirstOrDefaultAsync(k => k.klient_id == newPack.klient_id);

                if (klient == null)
                    return BadRequest($"Klient o id {newPack.klient_id} nie istnieje.");

                newPack.klient = klient;
            }

            _context.packs.Add(newPack);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPack), new { id = newPack.pack_id }, newPack);
        }

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
        [HttpPost("send")]
        public async Task<IActionResult> SendPack([FromBody] SendPackRequest request)
        {
            if (request.Size != "S" && request.Size != "M" && request.Size != "L")
                return BadRequest(new { message = "Nieprawidłowy rozmiar paczki." });

            if (string.IsNullOrWhiteSpace(request.ReceiverPhone))
                return BadRequest(new { message = "Numer telefonu odbiorcy jest wymagany." });

            if (!int.TryParse(request.ReceiverPhone.Replace(" ", ""), out int receiverPhone))
                return BadRequest(new { message = "Nieprawidłowy format numeru telefonu." });

            var receiverUser = await _context.users
                .FirstOrDefaultAsync(u => u.phone_number == receiverPhone);
            if (receiverUser == null)
                return BadRequest(new { message = "Nie znaleziono użytkownika o podanym numerze telefonu." });

            var receiverKlient = await _context.klients
                .FirstOrDefaultAsync(k => k.user_id == receiverUser.id);
            if (receiverKlient == null)
                return BadRequest(new { message = "Odbiorca nie jest klientem." });

            var senderUser = await _context.users
                .FirstOrDefaultAsync(u => u.email == request.ReceiverEmail);
            if (senderUser == null)
                return BadRequest(new { message = "Nie znaleziono nadawcy." });

            var senderKlient = await _context.klients
                .FirstOrDefaultAsync(k => k.user_id == senderUser.id);
            if (senderKlient == null)
                return BadRequest(new { message = "Nadawca nie jest klientem." });

            if (string.IsNullOrWhiteSpace(request.PaczkomatName))
                return BadRequest(new { message = "Nazwa paczkomatu jest wymagana." });

            var paczkomat = await _context.paczkomats
                .FirstOrDefaultAsync(p => p.paczkomat_name == request.PaczkomatName);
            if (paczkomat == null)
                return BadRequest(new { message = "Nie znaleziono paczkomatu." });

            var pending = new pending_pack
            {
                sender_klient_id = senderKlient.klient_id,
                receiver_klient_id = receiverKlient.klient_id,
                size = request.Size,
                paczkomat_id = paczkomat.paczkomat_id,
                created_at = DateTime.Now,
                status = "waiting"
            };

            _context.pending_packs.Add(pending);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Paczka została przyjęta i oczekuje na wydanie przez admina.",
                pendingId = pending.pending_id,
                receiverName = $"{receiverUser.name} {receiverUser.surname}",
                locker = paczkomat.paczkomat_name
            });
        }
    }
}
