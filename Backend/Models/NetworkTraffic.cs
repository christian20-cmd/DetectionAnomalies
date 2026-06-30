using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Représente les données brutes du trafic réseau capturé
    public class NetworkTraffic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Adresse IP source
        [Required]
        [MaxLength(45)]
        public string SourceIP { get; set; } = string.Empty;

        // Adresse IP destination
        [Required]
        [MaxLength(45)]
        public string DestinationIP { get; set; } = string.Empty;

        // Port source
        public int SourcePort { get; set; }

        // Port destination
        public int DestinationPort { get; set; }

        // Protocole réseau
        [MaxLength(10)]
        public string Protocol { get; set; } = string.Empty;

        // Taille moyenne des paquets en octets
        public float PacketSize { get; set; }

        // Nombre de connexions par seconde
        public float ConnectionsPerSecond { get; set; }

        // Nombre de ports différents contactés
        public int PortsContacted { get; set; }

        // Durée moyenne de session en secondes
        public float SessionDuration { get; set; }

        // Date et heure de capture
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        // --- Relation ---

        // Lien vers l'alerte générée (null si trafic normal)
        [ForeignKey("Alert")]
        public int? AlertId { get; set; }
        public Alert? Alert { get; set; }
    }
}