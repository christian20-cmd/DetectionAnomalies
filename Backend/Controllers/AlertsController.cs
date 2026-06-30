using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/alerts
        // Récupérer toutes les alertes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetAlerts()
        {
            var alerts = await _context.Alerts
                .Include(a => a.MLModel)
                .OrderByDescending(a => a.DetectedAt)
                .Select(a => new AlertResponseDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    SourceIP = a.SourceIP,
                    DestinationIP = a.DestinationIP,
                    Port = a.Port,
                    Protocol = a.Protocol,
                    Severity = a.Severity,
                    Confidence = a.Confidence,
                    DetectedAt = a.DetectedAt,
                    IsResolved = a.IsResolved,
                    ResolvedAt = a.ResolvedAt,
                    MLModelVersion = a.MLModel.Version
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // GET /api/alerts/5
        // Récupérer une alerte par son Id
        [HttpGet("{id}")]
        public async Task<ActionResult<AlertResponseDto>> GetAlert(int id)
        {
            var alert = await _context.Alerts
                .Include(a => a.MLModel)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
                return NotFound($"Alerte avec l'Id {id} introuvable.");

            var dto = new AlertResponseDto
            {
                Id = alert.Id,
                Type = alert.Type,
                SourceIP = alert.SourceIP,
                DestinationIP = alert.DestinationIP,
                Port = alert.Port,
                Protocol = alert.Protocol,
                Severity = alert.Severity,
                Confidence = alert.Confidence,
                DetectedAt = alert.DetectedAt,
                IsResolved = alert.IsResolved,
                ResolvedAt = alert.ResolvedAt,
                MLModelVersion = alert.MLModel.Version
            };

            return Ok(dto);
        }

        // GET /api/alerts/unresolved
        // Récupérer uniquement les alertes non traitées
        [HttpGet("unresolved")]
        public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetUnresolvedAlerts()
        {
            var alerts = await _context.Alerts
                .Include(a => a.MLModel)
                .Where(a => a.IsResolved == false)
                .OrderByDescending(a => a.DetectedAt)
                .Select(a => new AlertResponseDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    SourceIP = a.SourceIP,
                    DestinationIP = a.DestinationIP,
                    Port = a.Port,
                    Protocol = a.Protocol,
                    Severity = a.Severity,
                    Confidence = a.Confidence,
                    DetectedAt = a.DetectedAt,
                    IsResolved = a.IsResolved,
                    ResolvedAt = a.ResolvedAt,
                    MLModelVersion = a.MLModel.Version
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // GET /api/alerts/severity/Critique
        // Filtrer les alertes par niveau de criticité
        [HttpGet("severity/{severity}")]
        public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetAlertsBySeverity(string severity)
        {
            var alerts = await _context.Alerts
                .Include(a => a.MLModel)
                .Where(a => a.Severity == severity)
                .OrderByDescending(a => a.DetectedAt)
                .Select(a => new AlertResponseDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    SourceIP = a.SourceIP,
                    DestinationIP = a.DestinationIP,
                    Port = a.Port,
                    Protocol = a.Protocol,
                    Severity = a.Severity,
                    Confidence = a.Confidence,
                    DetectedAt = a.DetectedAt,
                    IsResolved = a.IsResolved,
                    ResolvedAt = a.ResolvedAt,
                    MLModelVersion = a.MLModel.Version
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // POST /api/alerts
        // Créer une nouvelle alerte (appelé par ML.NET)
        [HttpPost]
        public async Task<ActionResult<AlertResponseDto>> CreateAlert(AlertCreateDto dto)
        {
            // Vérifier que le modèle ML existe
            var mlModel = await _context.MLModels.FindAsync(dto.MLModelId);
            if (mlModel == null)
                return BadRequest($"Modèle ML avec l'Id {dto.MLModelId} introuvable.");

            var alert = new Alert
            {
                Type = dto.Type,
                SourceIP = dto.SourceIP,
                DestinationIP = dto.DestinationIP,
                Port = dto.Port,
                Protocol = dto.Protocol,
                Severity = dto.Severity,
                Confidence = dto.Confidence,
                MLModelId = dto.MLModelId,
                DetectedAt = DateTime.UtcNow,
                IsResolved = false
            };

            _context.Alerts.Add(alert);

            // Incrémenter le compteur d'anomalies du modèle ML
            mlModel.TotalAnomaliesDetected++;

            await _context.SaveChangesAsync();

            var responseDto = new AlertResponseDto
            {
                Id = alert.Id,
                Type = alert.Type,
                SourceIP = alert.SourceIP,
                DestinationIP = alert.DestinationIP,
                Port = alert.Port,
                Protocol = alert.Protocol,
                Severity = alert.Severity,
                Confidence = alert.Confidence,
                DetectedAt = alert.DetectedAt,
                IsResolved = alert.IsResolved,
                ResolvedAt = alert.ResolvedAt,
                MLModelVersion = mlModel.Version
            };

            return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, responseDto);
        }

        // PUT /api/alerts/5/resolve
        // Marquer une alerte comme résolue
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveAlert(int id, AlertResolveDto dto)
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return NotFound($"Alerte avec l'Id {id} introuvable.");

            alert.IsResolved = dto.IsResolved;
            alert.ResolvedAt = dto.IsResolved ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/alerts/5
        // Supprimer une alerte
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return NotFound($"Alerte avec l'Id {id} introuvable.");

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/alerts/stats
        // Statistiques pour le dashboard
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var stats = new
            {
                Total = await _context.Alerts.CountAsync(),
                Critique = await _context.Alerts.CountAsync(a => a.Severity == "Critique"),
                Moyen = await _context.Alerts.CountAsync(a => a.Severity == "Moyen"),
                Faible = await _context.Alerts.CountAsync(a => a.Severity == "Faible"),
                NonResolues = await _context.Alerts.CountAsync(a => a.IsResolved == false)
            };

            return Ok(stats);
        }
    }
}