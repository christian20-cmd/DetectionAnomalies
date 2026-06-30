using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkTrafficController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NetworkTrafficController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/networktraffic
        // Récupérer tout le trafic réseau
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NetworkTrafficResponseDto>>> GetAllTraffic()
        {
            var traffic = await _context.NetworkTraffics
                .OrderByDescending(nt => nt.CapturedAt)
                .Select(nt => new NetworkTrafficResponseDto
                {
                    Id = nt.Id,
                    SourceIP = nt.SourceIP,
                    DestinationIP = nt.DestinationIP,
                    SourcePort = nt.SourcePort,
                    DestinationPort = nt.DestinationPort,
                    Protocol = nt.Protocol,
                    PacketSize = nt.PacketSize,
                    ConnectionsPerSecond = nt.ConnectionsPerSecond,
                    PortsContacted = nt.PortsContacted,
                    SessionDuration = nt.SessionDuration,
                    CapturedAt = nt.CapturedAt,
                    AlertId = nt.AlertId
                })
                .ToListAsync();

            return Ok(traffic);
        }

        // GET /api/networktraffic/5
        // Récupérer un enregistrement par son Id
        [HttpGet("{id}")]
        public async Task<ActionResult<NetworkTrafficResponseDto>> GetTraffic(int id)
        {
            var traffic = await _context.NetworkTraffics
                .FirstOrDefaultAsync(nt => nt.Id == id);

            if (traffic == null)
                return NotFound($"Trafic réseau avec l'Id {id} introuvable.");

            var dto = new NetworkTrafficResponseDto
            {
                Id = traffic.Id,
                SourceIP = traffic.SourceIP,
                DestinationIP = traffic.DestinationIP,
                SourcePort = traffic.SourcePort,
                DestinationPort = traffic.DestinationPort,
                Protocol = traffic.Protocol,
                PacketSize = traffic.PacketSize,
                ConnectionsPerSecond = traffic.ConnectionsPerSecond,
                PortsContacted = traffic.PortsContacted,
                SessionDuration = traffic.SessionDuration,
                CapturedAt = traffic.CapturedAt,
                AlertId = traffic.AlertId
            };

            return Ok(dto);
        }

        // GET /api/networktraffic/suspicious
        // Récupérer uniquement le trafic qui a généré une alerte
        [HttpGet("suspicious")]
        public async Task<ActionResult<IEnumerable<NetworkTrafficResponseDto>>> GetSuspiciousTraffic()
        {
            var traffic = await _context.NetworkTraffics
                .Where(nt => nt.AlertId != null)
                .OrderByDescending(nt => nt.CapturedAt)
                .Select(nt => new NetworkTrafficResponseDto
                {
                    Id = nt.Id,
                    SourceIP = nt.SourceIP,
                    DestinationIP = nt.DestinationIP,
                    SourcePort = nt.SourcePort,
                    DestinationPort = nt.DestinationPort,
                    Protocol = nt.Protocol,
                    PacketSize = nt.PacketSize,
                    ConnectionsPerSecond = nt.ConnectionsPerSecond,
                    PortsContacted = nt.PortsContacted,
                    SessionDuration = nt.SessionDuration,
                    CapturedAt = nt.CapturedAt,
                    AlertId = nt.AlertId
                })
                .ToListAsync();

            return Ok(traffic);
        }

        // GET /api/networktraffic/ip/192.168.1.5
        // Récupérer tout le trafic d'une IP spécifique
        [HttpGet("ip/{sourceIP}")]
        public async Task<ActionResult<IEnumerable<NetworkTrafficResponseDto>>> GetTrafficByIP(string sourceIP)
        {
            var traffic = await _context.NetworkTraffics
                .Where(nt => nt.SourceIP == sourceIP)
                .OrderByDescending(nt => nt.CapturedAt)
                .Select(nt => new NetworkTrafficResponseDto
                {
                    Id = nt.Id,
                    SourceIP = nt.SourceIP,
                    DestinationIP = nt.DestinationIP,
                    SourcePort = nt.SourcePort,
                    DestinationPort = nt.DestinationPort,
                    Protocol = nt.Protocol,
                    PacketSize = nt.PacketSize,
                    ConnectionsPerSecond = nt.ConnectionsPerSecond,
                    PortsContacted = nt.PortsContacted,
                    SessionDuration = nt.SessionDuration,
                    CapturedAt = nt.CapturedAt,
                    AlertId = nt.AlertId
                })
                .ToListAsync();

            return Ok(traffic);
        }

        // GET /api/networktraffic/alert/5
        // Récupérer le trafic lié à une alerte spécifique
        [HttpGet("alert/{alertId}")]
        public async Task<ActionResult<IEnumerable<NetworkTrafficResponseDto>>> GetTrafficByAlert(int alertId)
        {
            var traffic = await _context.NetworkTraffics
                .Where(nt => nt.AlertId == alertId)
                .OrderByDescending(nt => nt.CapturedAt)
                .Select(nt => new NetworkTrafficResponseDto
                {
                    Id = nt.Id,
                    SourceIP = nt.SourceIP,
                    DestinationIP = nt.DestinationIP,
                    SourcePort = nt.SourcePort,
                    DestinationPort = nt.DestinationPort,
                    Protocol = nt.Protocol,
                    PacketSize = nt.PacketSize,
                    ConnectionsPerSecond = nt.ConnectionsPerSecond,
                    PortsContacted = nt.PortsContacted,
                    SessionDuration = nt.SessionDuration,
                    CapturedAt = nt.CapturedAt,
                    AlertId = nt.AlertId
                })
                .ToListAsync();

            return Ok(traffic);
        }

        // POST /api/networktraffic
        // Enregistrer un nouveau flux réseau capturé par SharpPcap
        [HttpPost]
        public async Task<ActionResult<NetworkTrafficResponseDto>> CreateTraffic(NetworkTrafficCreateDto dto)
        {
            var traffic = new NetworkTraffic
            {
                SourceIP = dto.SourceIP,
                DestinationIP = dto.DestinationIP,
                SourcePort = dto.SourcePort,
                DestinationPort = dto.DestinationPort,
                Protocol = dto.Protocol,
                PacketSize = dto.PacketSize,
                ConnectionsPerSecond = dto.ConnectionsPerSecond,
                PortsContacted = dto.PortsContacted,
                SessionDuration = dto.SessionDuration,
                CapturedAt = DateTime.UtcNow,
                AlertId = null
            };

            _context.NetworkTraffics.Add(traffic);
            await _context.SaveChangesAsync();

            var responseDto = new NetworkTrafficResponseDto
            {
                Id = traffic.Id,
                SourceIP = traffic.SourceIP,
                DestinationIP = traffic.DestinationIP,
                SourcePort = traffic.SourcePort,
                DestinationPort = traffic.DestinationPort,
                Protocol = traffic.Protocol,
                PacketSize = traffic.PacketSize,
                ConnectionsPerSecond = traffic.ConnectionsPerSecond,
                PortsContacted = traffic.PortsContacted,
                SessionDuration = traffic.SessionDuration,
                CapturedAt = traffic.CapturedAt,
                AlertId = traffic.AlertId
            };

            return CreatedAtAction(nameof(GetTraffic), new { id = traffic.Id }, responseDto);
        }

        // DELETE /api/networktraffic/5
        // Supprimer un enregistrement de trafic
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTraffic(int id)
        {
            var traffic = await _context.NetworkTraffics.FindAsync(id);

            if (traffic == null)
                return NotFound($"Trafic réseau avec l'Id {id} introuvable.");

            _context.NetworkTraffics.Remove(traffic);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/networktraffic/stats
        // Statistiques du trafic pour le dashboard
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var stats = new
            {
                TotalCaptures = await _context.NetworkTraffics.CountAsync(),
                TraficSuspect = await _context.NetworkTraffics.CountAsync(nt => nt.AlertId != null),
                TraficNormal = await _context.NetworkTraffics.CountAsync(nt => nt.AlertId == null),
                MoyenneConnectionsParSeconde = await _context.NetworkTraffics
                    .AverageAsync(nt => (double?)nt.ConnectionsPerSecond) ?? 0,
                MoyennePortsContactes = await _context.NetworkTraffics
                    .AverageAsync(nt => (double?)nt.PortsContacted) ?? 0
            };

            return Ok(stats);
        }
    }
}