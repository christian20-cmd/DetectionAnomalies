using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/users
        // Récupérer tous les utilisateurs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET /api/users/5
        // Récupérer un utilisateur par son Id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Utilisateur avec l'Id {id} introuvable.");

            var dto = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(dto);
        }

        // POST /api/users
        // Créer un nouvel utilisateur
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto dto)
        {
            // Vérifier que l'email n'existe pas déjà
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                return BadRequest($"Un utilisateur avec l'email {dto.Email} existe déjà.");

            // Vérifier que le username n'existe pas déjà
            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);

            if (usernameExists)
                return BadRequest($"Le nom d'utilisateur {dto.Username} est déjà pris.");

            // Hasher le mot de passe avec SHA256
            var passwordHash = HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var responseDto = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, responseDto);
        }

        // POST /api/users/login
        // Connexion d'un utilisateur
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            // Chercher l'utilisateur par email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Email ou mot de passe incorrect.");

            // Vérifier le mot de passe
            var passwordHash = HashPassword(dto.Password);
            if (user.PasswordHash != passwordHash)
                return Unauthorized("Email ou mot de passe incorrect.");

            // Mettre à jour la date de dernière connexion
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Pour l'instant on retourne un token simple
            // On ajoutera JWT complet dans AuthService plus tard
            var response = new LoginResponseDto
            {
                Token = GenerateSimpleToken(user),
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            return Ok(response);
        }

        // PUT /api/users/5
        // Mettre à jour un utilisateur
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserCreateDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Utilisateur avec l'Id {id} introuvable.");

            // Vérifier que le nouvel email n'appartient pas à un autre utilisateur
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (emailExists)
                return BadRequest($"L'email {dto.Email} est déjà utilisé par un autre compte.");

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.Role = dto.Role;

            // Mettre à jour le mot de passe seulement s'il est fourni
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/users/5
        // Supprimer un utilisateur
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Utilisateur avec l'Id {id} introuvable.");

            // On ne peut pas supprimer le dernier admin
            if (user.Role == "Admin")
            {
                var adminCount = await _context.Users
                    .CountAsync(u => u.Role == "Admin");

                if (adminCount <= 1)
                    return BadRequest("Impossible de supprimer le dernier administrateur.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Méthodes privées ---

        // Hasher le mot de passe avec SHA256
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Générer un token simple (sera remplacé par JWT dans AuthService)
        private string GenerateSimpleToken(User user)
        {
            var tokenData = $"{user.Id}:{user.Email}:{user.Role}:{DateTime.UtcNow}";
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(tokenData);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}