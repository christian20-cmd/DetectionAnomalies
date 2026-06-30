using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Représente une anomalie détectée par ML.NET
    public class Alert
    {
        // Identifiant unique auto-incrémenté
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Type de menace détectée
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        // Valeurs possibles : "DDoS", "Intrusion", "ScanPorts", "Malware"

        // Adresse IP de l'attaquant
        [Required]
        [MaxLength(45)]
        public string SourceIP { get; set; } = string.Empty;

        // Adresse IP de la machine ciblée
        [Required]
        [MaxLength(45)]
        public string DestinationIP { get; set; } = string.Empty;

        // Port ciblé
        public int Port { get; set; }

        // Protocole réseau utilisé
        [MaxLength(10)]
        public string Protocol { get; set; } = string.Empty;
        // Valeurs possibles : "TCP", "UDP", "ICMP"

        // Niveau de criticité
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty;
        // Valeurs possibles : "Faible", "Moyen", "Critique"

        // Score de confiance du modèle ML.NET (entre 0 et 1)
        public float Confidence { get; set; }

        // Date et heure de détection
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        // L'alerte a-t-elle été traitée par l'admin ?
        public bool IsResolved { get; set; } = false;

        // Date et heure de résolution
        public DateTime? ResolvedAt { get; set; }

        // --- Relations ---

        // Quel modèle ML.NET a généré cette alerte
        [ForeignKey("MLModel")]
        public int MLModelId { get; set; }
        public MLModel MLModel { get; set; } = null!;

        // Données réseau brutes qui ont déclenché cette alerte
        public ICollection<NetworkTraffic> NetworkTraffics { get; set; } 
            = new List<NetworkTraffic>();
    }
}