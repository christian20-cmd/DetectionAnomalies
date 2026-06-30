using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Backend.Hubs;

namespace Backend.Services
{
    public class AlertService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AlertHub> _hubContext;

        public AlertService(AppDbContext context, IHubContext<AlertHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Créer une nouvelle alerte et notifier le dashboard en temps réel
        public async Task<AlertResponseDto> CreateAlertAsync(AlertCreateDto dto)
        {
            // Vérifier que le modèle ML existe
            var mlModel = await _context.MLModels.FindAsync(dto.MLModelId);
            if (mlModel == null)
                throw new Exception($"Modèle ML avec l'Id {dto.MLModelId} introuvable.");

            // Créer l'alerte
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

            // Préparer le DTO de réponse
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

            // Notifier le dashboard en temps réel via SignalR
            await _hubContext.Clients.All.SendAsync("NewAlert", responseDto);

            return responseDto;
        }

        // Récupérer toutes les alertes
        public async Task<List<AlertResponseDto>> GetAllAlertsAsync()
        {
            return await _context.Alerts
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
        }

        // Récupérer les alertes non résolues
        public async Task<List<AlertResponseDto>> GetUnresolvedAlertsAsync()
        {
            return await _context.Alerts
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
        }

        // Résoudre une alerte
        public async Task<bool> ResolveAlertAsync(int id, bool isResolved)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return false;

            alert.IsResolved = isResolved;
            alert.ResolvedAt = isResolved ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            // Notifier le dashboard que l'alerte a été résolue
            await _hubContext.Clients.All.SendAsync("AlertResolved", new { id, isResolved });

            return true;
        }

        // Statistiques pour le dashboard
        public async Task<object> GetStatsAsync()
        {
            return new
            {
                Total = await _context.Alerts.CountAsync(),
                Critique = await _context.Alerts.CountAsync(a => a.Severity == "Critique"),
                Moyen = await _context.Alerts.CountAsync(a => a.Severity == "Moyen"),
                Faible = await _context.Alerts.CountAsync(a => a.Severity == "Faible"),
                NonResolues = await _context.Alerts.CountAsync(a => a.IsResolved == false)
            };
        }

        // Déterminer le niveau de criticité selon le type et le score de confiance
        public static string DetermineSeverity(string threatType, float confidence)
        {
            // Les attaques critiques par nature
            if (threatType == "DDoS" || threatType == "Intrusion" || threatType == "Malware")
            {
                if (confidence >= 0.85f) return "Critique";
                if (confidence >= 0.70f) return "Moyen";
                return "Faible";
            }

            // Scan de ports — moins critique
            if (threatType == "ScanPorts")
            {
                if (confidence >= 0.85f) return "Moyen";
                return "Faible";
            }

            return "Faible";
        }
    }
}