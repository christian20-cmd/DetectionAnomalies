using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Représente une adresse IP bloquée par un administrateur
    public class BlockedIP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Adresse IP bloquée
        [Required]
        [MaxLength(45)]
        public string IPAddress { get; set; } = string.Empty;

        // Raison du blocage
        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        // Date du blocage
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

        // Le blocage est-il encore actif ?
        public bool IsActive { get; set; } = true;

        // --- Relation ---

        // Quel admin a effectué le blocage
        [ForeignKey("User")]
        public int BlockedBy { get; set; }
        public User User { get; set; } = null!;
    }
}