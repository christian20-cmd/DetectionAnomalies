using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(AppDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Générer un rapport pour une période donnée
        public async Task<Report?> GenerateReportAsync(DateTime periodStart, DateTime periodEnd)
        {
            _logger.LogInformation($"📊 Génération du rapport du {periodStart:dd/MM/yyyy} au {periodEnd:dd/MM/yyyy}");

            // Récupérer toutes les alertes sur la période
            var alerts = await _context.Alerts
                .Where(a => a.DetectedAt >= periodStart && a.DetectedAt <= periodEnd)
                .ToListAsync();

            if (!alerts.Any())
            {
                _logger.LogWarning("⚠️ Aucune alerte trouvée sur cette période.");
                return null;
            }

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

            // Créer le rapport
            var report = new Report
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
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

            _logger.LogInformation($"✅ Rapport généré — {totalAnomalies} anomalies détectées.");

            return report;
        }

        // Générer un rapport quotidien automatiquement
        public async Task<Report?> GenerateDailyReportAsync()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var today = DateTime.UtcNow.Date;

            return await GenerateReportAsync(yesterday, today);
        }

        // Générer un rapport hebdomadaire automatiquement
        public async Task<Report?> GenerateWeeklyReportAsync()
        {
            var lastWeek = DateTime.UtcNow.Date.AddDays(-7);
            var today = DateTime.UtcNow.Date;

            return await GenerateReportAsync(lastWeek, today);
        }

        // Récupérer tous les rapports
        public async Task<List<Report>> GetAllReportsAsync()
        {
            return await _context.Reports
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }

        // Récupérer le rapport le plus récent
        public async Task<Report?> GetLatestReportAsync()
        {
            return await _context.Reports
                .OrderByDescending(r => r.GeneratedAt)
                .FirstOrDefaultAsync();
        }

        // Statistiques globales pour le dashboard
        public async Task<object> GetGlobalStatsAsync()
        {
            var totalAlerts = await _context.Alerts.CountAsync();
            var totalCritical = await _context.Alerts.CountAsync(a => a.Severity == "Critique");
            var totalUnresolved = await _context.Alerts.CountAsync(a => a.IsResolved == false);
            var totalBlockedIPs = await _context.BlockedIPs.CountAsync(b => b.IsActive == true);

            // Top 5 IPs suspectes
            var topIPs = await _context.Alerts
                .GroupBy(a => a.SourceIP)
                .Select(g => new { IP = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Top 5 types de menaces
            var topThreats = await _context.Alerts
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Alertes des 7 derniers jours par jour
            var last7Days = await _context.Alerts
                .Where(a => a.DetectedAt >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(a => a.DetectedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new
            {
                TotalAlerts = totalAlerts,
                TotalCritical = totalCritical,
                TotalUnresolved = totalUnresolved,
                TotalBlockedIPs = totalBlockedIPs,
                TopSuspiciousIPs = topIPs,
                TopThreatTypes = topThreats,
                Last7DaysActivity = last7Days
            };
        }
    }

    // Background service pour générer les rapports automatiquement
    public class ReportSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReportSchedulerService> _logger;

        public ReportSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReportSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🕐 ReportSchedulerService démarré.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Générer un rapport quotidien à minuit
                if (now.Hour == 0 && now.Minute == 0)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();
                    await reportService.GenerateDailyReportAsync();
                    _logger.LogInformation("✅ Rapport quotidien généré automatiquement.");
                }

                // Générer un rapport hebdomadaire le lundi à minuit
                if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 0 && now.Minute == 0)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();
                    await reportService.GenerateWeeklyReportAsync();
                    _logger.LogInformation("✅ Rapport hebdomadaire généré automatiquement.");
                }

                // Vérifier toutes les minutes
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}