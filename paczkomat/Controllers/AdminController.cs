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
    public class AdminController : ControllerBase
    {
        private readonly inpostContext _context;

        public AdminController(inpostContext context)
        {
            _context = context;
        }

        #region DTOs

        public class UnassignedPackDto
        {
            public int PackId { get; set; }
            public string Size { get; set; }
            public string Date { get; set; }
            public string ReceiverName { get; set; }
            public string PaczkomatName { get; set; }
            public bool HasKurier { get; set; }
        }

        public class KurierDto
        {
            public int KurierId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string State { get; set; }
            public int AssignedPacksCount { get; set; }
        }

        public class AssignPackRequest
        {
            public int PackId { get; set; }
            public int KurierId { get; set; }
        }

        public class PendingPackDto
        {
            public int PendingId { get; set; }
            public string Size { get; set; }
            public string CreatedAt { get; set; }
            public string Status { get; set; }
            public string SenderName { get; set; }
            public string ReceiverName { get; set; }
            public string PaczkomatName { get; set; }
        }

        #endregion

        [HttpGet("packs")]
        public async Task<ActionResult<IEnumerable<UnassignedPackDto>>> GetAllPacks()
        {
            var packs = await _context.packs
                .Include(p => p.klient)
                    .ThenInclude(k => k.user)
                .Where(p => p.delivered == false)
                .ToListAsync();

            var result = new List<UnassignedPackDto>();

            foreach (var pack in packs)
            {
                var hasKurier = await _context.kuriers_data
                    .AnyAsync(kd => kd.pack_id == pack.pack_id);

                var paczkomatName = await (from pd in _context.paczkomat_data
                                           join pac in _context.paczkomats on pd.paczkomat_id equals pac.paczkomat_id
                                           join b in _context.boxs on pd.box_id equals b.box_id
                                           where b.pack_id == pack.pack_id
                                           select pac.paczkomat_name)
                                          .FirstOrDefaultAsync();

                var receiverName = pack.klient?.user != null
                    ? $"{pack.klient.user.name} {pack.klient.user.surname}"
                    : "Nieznany";

                result.Add(new UnassignedPackDto
                {
                    PackId = pack.pack_id,
                    Size = pack.size ?? "?",
                    Date = pack.date?.ToString("yyyy-MM-dd") ?? "",
                    ReceiverName = receiverName,
                    PaczkomatName = paczkomatName ?? "Brak",
                    HasKurier = hasKurier
                });
            }

            return Ok(result);
        }

        [HttpGet("packs/unassigned")]
        public async Task<ActionResult<IEnumerable<UnassignedPackDto>>> GetUnassignedPacks()
        {
            var allPacks = await _context.packs
                .Include(p => p.klient)
                    .ThenInclude(k => k.user)
                .Where(p => p.delivered == false)
                .ToListAsync();

            var result = new List<UnassignedPackDto>();

            foreach (var pack in allPacks)
            {
                var hasKurier = await _context.kuriers_data
                    .AnyAsync(kd => kd.pack_id == pack.pack_id);

                if (hasKurier)
                    continue;

                var paczkomatName = await (from pd in _context.paczkomat_data
                                           join pac in _context.paczkomats on pd.paczkomat_id equals pac.paczkomat_id
                                           join b in _context.boxs on pd.box_id equals b.box_id
                                           where b.pack_id == pack.pack_id
                                           select pac.paczkomat_name)
                                          .FirstOrDefaultAsync();

                var receiverName = pack.klient?.user != null
                    ? $"{pack.klient.user.name} {pack.klient.user.surname}"
                    : "Nieznany";

                result.Add(new UnassignedPackDto
                {
                    PackId = pack.pack_id,
                    Size = pack.size ?? "?",
                    Date = pack.date?.ToString("yyyy-MM-dd") ?? "",
                    ReceiverName = receiverName,
                    PaczkomatName = paczkomatName ?? "Brak",
                    HasKurier = false
                });
            }

            return Ok(result);
        }

        [HttpGet("kuriers")]
        public async Task<ActionResult<IEnumerable<KurierDto>>> GetKuriers()
        {
            var kuriers = await _context.kuriers
                .Include(k => k.user)
                .Where(k => k.user != null)
                .ToListAsync();

            var result = new List<KurierDto>();

            foreach (var kurier in kuriers)
            {
                var assignedPacksCount = await _context.kuriers_data
                    .Where(kd => kd.kurier_id == kurier.kurier_id)
                    .Join(_context.packs,
                          kd => kd.pack_id,
                          p => p.pack_id,
                          (kd, p) => p)
                    .Where(p => p.delivered == false)
                    .CountAsync();

                result.Add(new KurierDto
                {
                    KurierId = kurier.kurier_id,
                    Name = kurier.user != null
                        ? $"{kurier.user.name} {kurier.user.surname}"
                        : "Nieznany",
                    Email = kurier.user?.email ?? "",
                    State = kurier.state ?? "available",
                    AssignedPacksCount = assignedPacksCount
                });
            }

            return Ok(result);
        }

        [HttpPost("assign-pack")]
        public async Task<IActionResult> AssignPackToKurier([FromBody] AssignPackRequest request)
        {
            var pack = await _context.packs.FindAsync(request.PackId);
            if (pack == null)
                return NotFound(new { message = "Paczka nie istnieje." });

            var kurier = await _context.kuriers.FindAsync(request.KurierId);
            if (kurier == null)
                return NotFound(new { message = "Kurier nie istnieje." });

            var existingAssignment = await _context.kuriers_data
                .FirstOrDefaultAsync(kd => kd.pack_id == request.PackId);

            if (existingAssignment != null)
            {
                existingAssignment.kurier_id = request.KurierId;
            }
            else
            {
                var newAssignment = new kuriers_datum
                {
                    kurier_id = request.KurierId,
                    pack_id = request.PackId
                };
                _context.kuriers_data.Add(newAssignment);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Paczka została przypisana do kuriera." });
        }

        [HttpDelete("unassign-pack/{packId}")]
        public async Task<IActionResult> UnassignPack(int packId)
        {
            var assignment = await _context.kuriers_data
                .FirstOrDefaultAsync(kd => kd.pack_id == packId);

            if (assignment == null)
                return NotFound(new { message = "Paczka nie ma przypisanego kuriera." });

            _context.kuriers_data.Remove(assignment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kurier został usunięty z paczki." });
        }

        [HttpDelete("delete-pack/{packId}")]
        public async Task<IActionResult> DeletePack(int packId)
        {
            var pack = await _context.packs.FindAsync(packId);
            if (pack == null)
                return NotFound(new { message = "Nie znaleziono paczki." });

            // Usuń przypisanie kuriera jeśli istnieje
            var assignment = await _context.kuriers_data
                .FirstOrDefaultAsync(kd => kd.pack_id == packId);
            if (assignment != null)
                _context.kuriers_data.Remove(assignment);

            // Usuń powiązania box → paczkomat_data (raw SQL bo paczkomat_data nie ma klucza głównego)
            var boxList = await _context.boxs
                .Where(b => b.pack_id == packId)
                .ToListAsync();
            foreach (var box in boxList)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM paczkomat_data WHERE box_id = {0}", box.box_id);
            }
            _context.boxs.RemoveRange(boxList);

            _context.packs.Remove(pack);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Paczka została usunięta." });
        }

        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var totalPacks = await _context.packs.CountAsync();
            var undeliveredPacks = await _context.packs.CountAsync(p => p.delivered == false);
            var deliveredPacks = await _context.packs.CountAsync(p => p.delivered == true);

            var unassignedPacks = 0;
            var allUndelivered = await _context.packs
                .Where(p => p.delivered == false)
                .Select(p => p.pack_id)
                .ToListAsync();

            foreach (var packId in allUndelivered)
            {
                var hasKurier = await _context.kuriers_data
                    .AnyAsync(kd => kd.pack_id == packId);
                if (!hasKurier)
                    unassignedPacks++;
            }

            var totalKuriers = await _context.kuriers
                .Include(k => k.user)
                .CountAsync(k => k.user != null);
            var activeKuriers = await _context.kuriers
                .Include(k => k.user)
                .CountAsync(k => k.user != null && k.state == "available");
            var pendingPacks = await _context.pending_packs.CountAsync(p => p.status == "waiting");

            return Ok(new
            {
                totalPacks,
                undeliveredPacks,
                deliveredPacks,
                unassignedPacks,
                totalKuriers,
                activeKuriers,
                pendingPacks
            });
        }

        [HttpGet("pending-packs")]
        public async Task<ActionResult<IEnumerable<PendingPackDto>>> GetPendingPacks()
        {
            var pending = await _context.pending_packs
                .Include(p => p.sender_klient).ThenInclude(k => k.user)
                .Include(p => p.receiver_klient).ThenInclude(k => k.user)
                .Include(p => p.paczkomat)
                .Where(p => p.status == "waiting")
                .ToListAsync();

            var result = pending.Select(p => new PendingPackDto
            {
                PendingId = p.pending_id,
                Size = p.size ?? "?",
                CreatedAt = p.created_at?.ToString("yyyy-MM-dd HH:mm") ?? "",
                Status = p.status ?? "waiting",
                SenderName = p.sender_klient?.user != null
                    ? $"{p.sender_klient.user.name} {p.sender_klient.user.surname}"
                    : "Nieznany",
                ReceiverName = p.receiver_klient?.user != null
                    ? $"{p.receiver_klient.user.name} {p.receiver_klient.user.surname}"
                    : "Nieznany",
                PaczkomatName = p.paczkomat?.paczkomat_name ?? "Brak"
            }).ToList();

            return Ok(result);
        }

        [HttpPost("release-pack/{pendingId}")]
        public async Task<IActionResult> ReleasePack(int pendingId)
        {
            var pending = await _context.pending_packs
                .Include(p => p.receiver_klient).ThenInclude(k => k.user)
                .Include(p => p.paczkomat)
                .FirstOrDefaultAsync(p => p.pending_id == pendingId);

            if (pending == null)
                return NotFound(new { message = "Nie znaleziono oczekującej paczki." });

            if (pending.status != "waiting")
                return BadRequest(new { message = "Paczka została już wydana." });

            var maxPackId = await _context.packs.AnyAsync()
                ? await _context.packs.MaxAsync(p => p.pack_id)
                : 0;

            var newPack = new pack
            {
                pack_id = maxPackId + 1,
                size = pending.size,
                klient_id = pending.receiver_klient_id,
                delivered = false,
                date = DateOnly.FromDateTime(DateTime.Today),
                to_when = DateOnly.FromDateTime(DateTime.Today.AddDays(3))
            };
            _context.packs.Add(newPack);
            await _context.SaveChangesAsync();

            var newBox = new box
            {
                pack_id = newPack.pack_id,
                size = pending.size
            };
            _context.boxs.Add(newBox);
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO paczkomat_data (paczkomat_id, box_id) VALUES ({0}, {1})",
                pending.paczkomat_id, newBox.box_id);

            pending.status = "released";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Paczka została wydana.",
                packId = newPack.pack_id,
                receiverName = pending.receiver_klient?.user != null
                    ? $"{pending.receiver_klient.user.name} {pending.receiver_klient.user.surname}"
                    : "Nieznany"
            });
        }

        [HttpPut("kurier/{kurierId}/toggle-state")]
        public async Task<IActionResult> ToggleKurierState(int kurierId)
        {
            var kurier = await _context.kuriers.FindAsync(kurierId);
            if (kurier == null)
                return NotFound(new { message = "Kurier nie istnieje." });

            kurier.state = (kurier.state == "available") ? "unavailable" : "available";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status zmieniony.", state = kurier.state });
        }

        [HttpDelete("reject-pack/{pendingId}")]
        public async Task<IActionResult> RejectPack(int pendingId)
        {
            var pending = await _context.pending_packs
                .FirstOrDefaultAsync(p => p.pending_id == pendingId);

            if (pending == null)
                return NotFound(new { message = "Nie znaleziono oczekującej paczki." });

            if (pending.status != "waiting")
                return BadRequest(new { message = "Paczka nie jest w stanie oczekiwania." });

            pending.status = "rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Paczka została odrzucona." });
        }
    }
}