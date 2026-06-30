using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Représente un rapport d'analyse périodique
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Début de la période analysée
        public DateTime PeriodStart { get; set; }

        // Fin de la période analysée
        public DateTime PeriodEnd { get; set; }

        // Nombre total d'anomalies sur la période
        public int TotalAnomalies { get; set; }

        // Nombre d'alertes critiques
        public int CriticalCount { get; set; }

        // Nombre d'alertes moyennes
        public int MediumCount { get; set; }

        // Nombre d'alertes faibles
        public int LowCount { get; set; }

        // Menace la plus fréquente sur la période
        [MaxLength(50)]
        public string TopThreatType { get; set; } = string.Empty;

        // IP la plus suspecte sur la période
        [MaxLength(45)]
        public string TopSourceIP { get; set; } = string.Empty;

        // Date de génération du rapport
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Chemin du fichier PDF ou Excel généré
        [MaxLength(255)]
        public string? FilePath { get; set; }
    }
}