using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/reports
        // Récupérer tous les rapports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports()
        {
            var reports = await _context.Reports
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();

            return Ok(reports);
        }

        // GET /api/reports/5
        // Récupérer un rapport par son Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
                return NotFound($"Rapport avec l'Id {id} introuvable.");

            return Ok(report);
        }

        // GET /api/reports/latest
        // Récupérer le rapport le plus récent
        [HttpGet("latest")]
        public async Task<ActionResult<Report>> GetLatestReport()
        {
            var report = await _context.Reports
                .OrderByDescending(r => r.GeneratedAt)
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound("Aucun rapport disponible.");

            return Ok(report);
        }

        // POST /api/reports/generate
        // Générer un nouveau rapport pour une période donnée
        [HttpPost("generate")]
        public async Task<ActionResult<Report>> GenerateReport([FromBody] GenerateReportDto dto)
        {
            // Récupérer toutes les alertes sur la période
            var alerts = await _context.Alerts
                .Where(a => a.DetectedAt >= dto.PeriodStart && a.DetectedAt <= dto.PeriodEnd)
                .ToListAsync();

            if (!alerts.Any())
                return BadRequest("Aucune alerte trouvée sur cette période.");

            // Calculer les statistiques
            var totalAnomalies = alerts.Count;
            var criticalCount = alerts.Count(a => a.Severity == "Critique");
            var mediumCount = alerts.Count(a => a.Severity == "Moyen");
            var lowCount = alerts.Count(a => a.Severity == "Faible");

            // Trouver la menace la plus fréquente
            var topThreatType = alerts
                .GroupBy(a => a.Type)
                .OrderByDescending(g => g.Count())
                .First().Key;

            // Trouver l'IP la plus suspecte
            var topSourceIP = alerts
                .GroupBy(a => a.SourceIP)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var report = new Report
            {
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                TotalAnomalies = totalAnomalies,
                CriticalCount = criticalCount,
                MediumCount = mediumCount,
                LowCount = lowCount,
                TopThreatType = topThreatType,
                TopSourceIP = topSourceIP,
                GeneratedAt = DateTime.UtcNow,
                FilePath = null
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }

        // DELETE /api/reports/5
        // Supprimer un rapport
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
                return NotFound($"Rapport avec l'Id {id} introuvable.");

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO local pour la génération de rapport
    public class GenerateReportDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}