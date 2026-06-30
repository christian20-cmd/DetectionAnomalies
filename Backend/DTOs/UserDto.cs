namespace Backend.DTOs
{
    // DTO pour envoyer les infos d'un utilisateur au frontend (GET)
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        // PasswordHash est volontairement absent — jamais envoyé au frontend
    }

    // DTO pour créer un utilisateur (POST)
    public class UserCreateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Le mot de passe en clair ici — AuthService va le hasher avant de sauvegarder
        public string Role { get; set; } = "Viewer";
    }

    // DTO pour la connexion (POST /api/auth/login)
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO pour la réponse après connexion réussie
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        // Le token JWT que React va stocker et envoyer à chaque requête
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}