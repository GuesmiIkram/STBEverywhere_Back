
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using STBEverywhere_Back_SharedModels.Models;


namespace STBEverywhere_Back_SharedModels.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Définir les DbSets pour toutes les entités
        public DbSet<Client> Clients { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Virement> Virements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'entité Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nom).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Prenom).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Telephone).IsRequired().HasMaxLength(20);
                entity.Property(c => c.Adresse).IsRequired().HasMaxLength(200);
                entity.Property(c => c.MotDePasse).IsRequired().HasMaxLength(100);

                // Ajouter des données initiales pour les clients
                entity.HasData(
                    new Client { Id = 1, Nom = "Doe", Prenom = "John", DateNaissance = new DateTime(1980, 1, 1), Telephone = "123456789", Email = "john.doe@example.com", Adresse = "123 Main St", Civilite = "M", Nationalite = "US", EtatCivil = "Célibataire", Residence = "New York", NumCIN = "A123456", MotDePasse = BCrypt.Net.BCrypt.HashPassword("password123") },
                    new Client { Id = 2, Nom = "Smith", Prenom = "Jane", DateNaissance = new DateTime(1990, 5, 15), Telephone = "987654321", Email = "jane.smith@example.com", Adresse = "456 Elm St", Civilite = "Mme", Nationalite = "CA", EtatCivil = "Marié(e)", Residence = "Toronto", NumCIN = "B654321", MotDePasse = BCrypt.Net.BCrypt.HashPassword("password456") }
                );
            });

            // Configuration de l'entité Compte
            modelBuilder.Entity<Compte>(entity =>
            {
                entity.HasKey(c => c.RIB);
                entity.Property(c => c.Type).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Solde).HasColumnType("decimal(18,3)");
                entity.Property(c => c.Statut).HasMaxLength(20);

                // Ajouter des données initiales pour les comptes
                entity.HasData(
                    new Compte { RIB = "12345678923537902652", NumCin = "14668061", Type = "Courant", Solde = 1000.50m, DateCreation = new DateTime(2024, 1, 1), Statut = "Actif", ClientId = 1 },
                    new Compte { RIB = "65432110223463790345", NumCin = "14668062", Type = "Épargne", Solde = 5000.00m, DateCreation = new DateTime(2024, 1, 1), Statut = "Actif", ClientId = 1 }
                );
            });

            // Configuration de l'entité Virement
            modelBuilder.Entity<Virement>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasIndex(v => new { v.RIB_Emetteur, v.DateVirement }).IsUnique();
            });
        }
    }
}
