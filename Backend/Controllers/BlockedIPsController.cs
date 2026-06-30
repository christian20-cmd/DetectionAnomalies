using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockedIPsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlockedIPsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/blockedips
        // Récupérer toutes les IPs bloquées
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllBlockedIPs()
        {
            var blockedIPs = await _context.BlockedIPs
                .Include(b => b.User)
                .OrderByDescending(b => b.BlockedAt)
                .Select(b => new
                {
                    b.Id,
                    b.IPAddress,
                    b.Reason,
                    b.BlockedAt,
                    b.IsActive,
                    BlockedBy = b.User.Username
                })
                .ToListAsync();

            return Ok(blockedIPs);
        }

        // GET /api/blockedips/active
        // Récupérer uniquement les IPs actuellement bloquées
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<object>>> GetActiveBlockedIPs()
        {
            var blockedIPs = await _context.BlockedIPs
                .Include(b => b.User)
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.BlockedAt)
                .Select(b => new
                {
                    b.Id,
                    b.IPAddress,
                    b.Reason,
                    b.BlockedAt,
                    b.IsActive,
                    BlockedBy = b.User.Username
                })
                .ToListAsync();

            return Ok(blockedIPs);
        }

        // GET /api/blockedips/5
        // Récupérer une IP bloquée par son Id
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBlockedIP(int id)
        {
            var blockedIP = await _context.BlockedIPs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blockedIP == null)
                return NotFound($"IP bloquée avec l'Id {id} introuvable.");

            var result = new
            {
                blockedIP.Id,
                blockedIP.IPAddress,
                blockedIP.Reason,
                blockedIP.BlockedAt,
                blockedIP.IsActive,
                BlockedBy = blockedIP.User.Username
            };

            return Ok(result);
        }

        // GET /api/blockedips/check/192.168.1.5
        // Vérifier si une IP est bloquée
        [HttpGet("check/{ipAddress}")]
        public async Task<ActionResult<object>> CheckIP(string ipAddress)
        {
            var blockedIP = await _context.BlockedIPs
                .FirstOrDefaultAsync(b => b.IPAddress == ipAddress && b.IsActive == true);

            var result = new
            {
                IPAddress = ipAddress,
                IsBlocked = blockedIP != null,
                Reason = blockedIP?.Reason ?? string.Empty,
                BlockedAt = blockedIP?.BlockedAt
            };

            return Ok(result);
        }

        // POST /api/blockedips
        // Bloquer une nouvelle IP
        [HttpPost]
        public async Task<ActionResult<object>> BlockIP([FromBody] BlockIPDto dto)
        {
            // Vérifier que l'utilisateur existe
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest($"Utilisateur avec l'Id {dto.UserId} introuvable.");

            // Vérifier si l'IP est déjà bloquée
            var alreadyBlocked = await _context.BlockedIPs
                .AnyAsync(b => b.IPAddress == dto.IPAddress && b.IsActive == true);

            if (alreadyBlocked)
                return BadRequest($"L'IP {dto.IPAddress} est déjà bloquée.");

            var blockedIP = new BlockedIP
            {
                IPAddress = dto.IPAddress,
                Reason = dto.Reason,
                BlockedAt = DateTime.UtcNow,
                IsActive = true,
                BlockedBy = dto.UserId
            };

            _context.BlockedIPs.Add(blockedIP);
            await _context.SaveChangesAsync();

            var result = new
            {
                blockedIP.Id,
                blockedIP.IPAddress,
                blockedIP.Reason,
                blockedIP.BlockedAt,
                blockedIP.IsActive,
                BlockedBy = user.Username
            };

            return CreatedAtAction(nameof(GetBlockedIP), new { id = blockedIP.Id }, result);
        }

        // PUT /api/blockedips/5/unblock
        // Débloquer une IP
        [HttpPut("{id}/unblock")]
        public async Task<IActionResult> UnblockIP(int id)
        {
            var blockedIP = await _context.BlockedIPs.FindAsync(id);

            if (blockedIP == null)
                return NotFound($"IP bloquée avec l'Id {id} introuvable.");

            if (!blockedIP.IsActive)
                return BadRequest("Cette IP est déjà débloquée.");

            blockedIP.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/blockedips/5
        // Supprimer un enregistrement de blocage
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlockedIP(int id)
        {
            var blockedIP = await _context.BlockedIPs.FindAsync(id);

            if (blockedIP == null)
                return NotFound($"IP bloquée avec l'Id {id} introuvable.");

            _context.BlockedIPs.Remove(blockedIP);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/blockedips/stats
        // Statistiques des IPs bloquées
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var stats = new
            {
                TotalBloquees = await _context.BlockedIPs.CountAsync(),
                ActivesBloquees = await _context.BlockedIPs.CountAsync(b => b.IsActive == true),
                Debloquees = await _context.BlockedIPs.CountAsync(b => b.IsActive == false)
            };

            return Ok(stats);
        }
    }

    // DTO local pour bloquer une IP
    public class BlockIPDto
    {
        public string IPAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}