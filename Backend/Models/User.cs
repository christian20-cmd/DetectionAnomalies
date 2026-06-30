using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Représente un administrateur du système
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Nom d'utilisateur
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        // Adresse email
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        // Mot de passe hashé (jamais en clair)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Rôle de l'utilisateur
        [MaxLength(20)]
        public string Role { get; set; } = "Viewer";
        // Valeurs possibles : "Admin", "Viewer"

        // Date de création du compte
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Dernière connexion
        public DateTime? LastLoginAt { get; set; }

        // --- Relation ---

        // IPs bloquées par cet admin
        public ICollection<BlockedIP> BlockedIPs { get; set; } 
            = new List<BlockedIP>();
    }
}