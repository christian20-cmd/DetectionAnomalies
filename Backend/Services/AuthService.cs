using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Connexion — vérifie les credentials et retourne un JWT
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            // Chercher l'utilisateur par email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return null;

            // Vérifier le mot de passe
            if (!VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            // Mettre à jour la dernière connexion
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Générer le token JWT
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpiresInMinutes")
            );

            return new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        // Créer un nouvel utilisateur avec mot de passe hashé
        public async Task<UserResponseDto?> RegisterAsync(UserCreateDto dto)
        {
            // Vérifier que l'email n'existe pas déjà
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);
            if (emailExists) return null;

            // Vérifier que le username n'existe pas déjà
            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);
            if (usernameExists) return null;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        // Hasher le mot de passe avec BCrypt
        public string HashPassword(string password)
        {
            // Générer un salt aléatoire
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hasher avec PBKDF2 — plus sécurisé que SHA256
            var hash = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000, // 100 000 itérations
                HashAlgorithmName.SHA256
            );

            byte[] hashBytes = hash.GetBytes(32);

            // Combiner salt + hash pour le stockage
            byte[] combined = new byte[48];
            Array.Copy(salt, 0, combined, 0, 16);
            Array.Copy(hashBytes, 0, combined, 16, 32);

            return Convert.ToBase64String(combined);
        }

        // Vérifier le mot de passe
        public bool VerifyPassword(string password, string storedHash)
        {
            byte[] combined = Convert.FromBase64String(storedHash);

            // Extraire le salt (les 16 premiers octets)
            byte[] salt = new byte[16];
            Array.Copy(combined, 0, salt, 0, 16);

            // Extraire le hash (les 32 octets suivants)
            byte[] storedHashBytes = new byte[32];
            Array.Copy(combined, 16, storedHashBytes, 0, 32);

            // Recalculer le hash avec le même salt
            var hash = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256
            );

            byte[] computedHash = hash.GetBytes(32);

            // Comparer les deux hash
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
        }

        // Générer un token JWT
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"]!;
            var jwtIssuer = _configuration["Jwt:Issuer"]!;
            var jwtAudience = _configuration["Jwt:Audience"]!;
            var expiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Les claims sont les informations stockées dans le token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Valider un token JWT
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                // Token invalide ou expiré
                return null;
            }
        }
    }
}