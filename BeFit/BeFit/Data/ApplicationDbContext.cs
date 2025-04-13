using Microsoft.AspNetCore.Identity; // Potrzebne dla IdentityUser
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BeFit.Models; // Przestrzeń nazw dla Twoich modeli

namespace BeFit.Data
{
    // Definicja kontekstu bazy danych aplikacji, dziedzicząca z IdentityDbContext.
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // Konstruktor klasy, przyjmujący opcje konfiguracji DbContext.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // Wywołanie konstruktora klasy bazowej.
        {
        }

        // Reprezentuje tabelę 'Exercises' w bazie danych.
        public DbSet<Exercise> Exercises { get; set; }
        // Reprezentuje tabelę 'TrainingSessions' w bazie danych.
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        // Reprezentuje tabelę 'TrainingDetails' w bazie danych.
        public DbSet<TrainingDetail> TrainingDetails { get; set; }

        // Metoda wywoływana podczas tworzenia modelu bazy danych przez EF Core.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Wywołanie metody bazowej jest wymagane dla konfiguracji Identity.
            base.OnModelCreating(builder);

            // Konfiguracja encji TrainingDetail za pomocą Fluent API.
            builder.Entity<TrainingDetail>(entity =>
            {
                // Ustawia typ kolumny dla właściwości 'Load' na decimal(6, 2) w bazie danych.
                entity.Property(e => e.Load).HasColumnType("decimal(6, 2)");
            });
        }
    }
}
