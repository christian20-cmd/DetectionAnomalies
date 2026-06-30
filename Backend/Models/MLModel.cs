using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Représente une version du modèle ML.NET
    public class MLModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Version du modèle
        [Required]
        [MaxLength(20)]
        public string Version { get; set; } = string.Empty;
        // Exemple : "v1.0", "v2.0"

        // Algorithme utilisé
        [Required]
        [MaxLength(50)]
        public string Algorithm { get; set; } = string.Empty;
        // Exemple : "RandomForest", "FastTree", "LightGbm"

        // Précision globale du modèle
        public float Accuracy { get; set; }

        // Taux de vrais positifs
        public float Precision { get; set; }

        // Taux de détection
        public float Recall { get; set; }

        // Equilibre précision/recall
        public float F1Score { get; set; }

        // Dataset utilisé pour l'entraînement
        [MaxLength(100)]
        public string TrainingDataset { get; set; } = string.Empty;
        // Exemple : "CICIDS2017"

        // Date d'entraînement
        public DateTime TrainingDate { get; set; } = DateTime.UtcNow;

        // Ce modèle est-il actuellement actif ?
        public bool IsActive { get; set; } = false;

        // Chemin du fichier .zip du modèle sauvegardé
        [MaxLength(255)]
        public string ModelFilePath { get; set; } = string.Empty;

        // Nombre total d'anomalies détectées par ce modèle
        public int TotalAnomaliesDetected { get; set; } = 0;

        // Remarques sur cette version
        [MaxLength(500)]
        public string? Notes { get; set; }

        // --- Relation ---

        // Alertes générées par ce modèle
        public ICollection<Alert> Alerts { get; set; } 
            = new List<Alert>();
    }
}