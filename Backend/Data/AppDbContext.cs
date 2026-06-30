using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    // Classe principale qui fait le lien entre les Models C# et PostgreSQL
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        // --- Les tables de la base de données ---
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<NetworkTraffic> NetworkTraffics { get; set; }
        public DbSet<MLModel> MLModels { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BlockedIP> BlockedIPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuration de la table Alerts ---
            modelBuilder.Entity<Alert>(entity =>
            {
                // Nom de la table dans PostgreSQL
                entity.ToTable("Alerts");

                // Index sur SourceIP pour accélérer les recherches
                entity.HasIndex(a => a.SourceIP)
                    .HasDatabaseName("IX_Alerts_SourceIP");

                // Index sur DetectedAt pour les tris chronologiques
                entity.HasIndex(a => a.DetectedAt)
                    .HasDatabaseName("IX_Alerts_DetectedAt");

                // Index sur Severity pour filtrer par criticité
                entity.HasIndex(a => a.Severity)
                    .HasDatabaseName("IX_Alerts_Severity");

                // Relation Alert → MLModel
                // Une alerte appartient à un seul modèle ML
                entity.HasOne(a => a.MLModel)
                    .WithMany(m => m.Alerts)
                    .HasForeignKey(a => a.MLModelId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Restrict : on ne peut pas supprimer un modèle ML
                // s'il a des alertes associées
            });

            // --- Configuration de la table NetworkTraffics ---
            modelBuilder.Entity<NetworkTraffic>(entity =>
            {
                entity.ToTable("NetworkTraffics");

                // Index sur CapturedAt pour les analyses temporelles
                entity.HasIndex(nt => nt.CapturedAt)
                    .HasDatabaseName("IX_NetworkTraffics_CapturedAt");

                // Index sur SourceIP pour les recherches par IP
                entity.HasIndex(nt => nt.SourceIP)
                    .HasDatabaseName("IX_NetworkTraffics_SourceIP");

                // Relation NetworkTraffic → Alert
                // Un trafic peut être lié à une alerte (ou null si normal)
                entity.HasOne(nt => nt.Alert)
                    .WithMany(a => a.NetworkTraffics)
                    .HasForeignKey(nt => nt.AlertId)
                    .OnDelete(DeleteBehavior.SetNull);
                // SetNull : si l'alerte est supprimée,
                // AlertId devient null dans NetworkTraffic
            });

            // --- Configuration de la table MLModels ---
            modelBuilder.Entity<MLModel>(entity =>
            {
                entity.ToTable("MLModels");

                // La version doit être unique
                entity.HasIndex(m => m.Version)
                    .IsUnique()
                    .HasDatabaseName("IX_MLModels_Version");

                // Index sur IsActive pour retrouver rapidement le modèle actif
                entity.HasIndex(m => m.IsActive)
                    .HasDatabaseName("IX_MLModels_IsActive");
            });

            // --- Configuration de la table Reports ---
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Reports");

                // Index sur GeneratedAt pour trier les rapports
                entity.HasIndex(r => r.GeneratedAt)
                    .HasDatabaseName("IX_Reports_GeneratedAt");
            });

            // --- Configuration de la table Users ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                // L'email doit être unique
                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                // Le username doit être unique
                entity.HasIndex(u => u.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");
            });

            // --- Configuration de la table BlockedIPs ---
            modelBuilder.Entity<BlockedIP>(entity =>
            {
                entity.ToTable("BlockedIPs");

                // Index sur IPAddress pour les recherches rapides
                entity.HasIndex(b => b.IPAddress)
                    .HasDatabaseName("IX_BlockedIPs_IPAddress");

                // Index sur IsActive pour filtrer les blocages actifs
                entity.HasIndex(b => b.IsActive)
                    .HasDatabaseName("IX_BlockedIPs_IsActive");

                // Relation BlockedIP → User
                // Un blocage appartient à un seul admin
                entity.HasOne(b => b.User)
                    .WithMany(u => u.BlockedIPs)
                    .HasForeignKey(b => b.BlockedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                // Restrict : on ne peut pas supprimer un admin
                // s'il a des IPs bloquées associées
            });

            // --- Données initiales (Seed Data) ---
            // Un admin par défaut au démarrage du projet
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@detection.com",
                    // Le mot de passe sera hashé dans le AuthService
                    PasswordHash = "CHANGE_THIS_HASH",
                    Role = "Admin",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Un premier modèle ML par défaut
            modelBuilder.Entity<MLModel>().HasData(
                new MLModel
                {
                    Id = 1,
                    Version = "v1.0",
                    Algorithm = "FastTree",
                    Accuracy = 0.0f,
                    Precision = 0.0f,
                    Recall = 0.0f,
                    F1Score = 0.0f,
                    TrainingDataset = "CICIDS2017",
                    TrainingDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    ModelFilePath = "MLModels/model_v1.zip",
                    TotalAnomaliesDetected = 0,
                    Notes = "Modèle initial du projet"
                }
            );
        }
    }
}