using Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    internal class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Alert> Alerts { get; set; } = null!;
        public DbSet<Batch> Batches { get; set; } = null!;
        public DbSet<Medicine> Medicines { get; set; } = null!;
        public DbSet<Reading> Readings { get; set; } = null!;
        public DbSet<Sensor> Sensors { get; set; } = null!;
        public DbSet<Zone> Zones { get; set; } = null!;
        public DbSet<SensorApiKey> SensorApiKeys { get; set; } = null!;
        public DbSet<AppSettings> AppSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CONFIG SETTINGS SCHEMA ==

            modelBuilder.Entity<AppSettings>(builder =>
            {
                builder.ToTable("Settings", "config");
            });

            ConfigureDomain(modelBuilder);
        }

        private void ConfigureDomain(ModelBuilder modelBuilder)
        {
            // Medicine
            modelBuilder.Entity<Medicine>(e =>
            {
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).HasMaxLength(1000);
            });

            // Zone
            modelBuilder.Entity<Zone>(e =>
            {
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).HasMaxLength(1000);
            });

            // Batch
            modelBuilder.Entity<Batch>(e =>
            {
                e.HasOne(b => b.Medicine)
                    .WithMany(m => m.Batches)
                    .HasForeignKey(b => b.MedicineId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.Zone)
                    .WithMany(z => z.Batches)
                    .HasForeignKey(b => b.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Sensor
            modelBuilder.Entity<Sensor>(e =>
            {
                e.HasOne(s => s.Zone)
                    .WithMany(z => z.Sensors)
                    .HasForeignKey(s => s.ZoneId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Sensor
            modelBuilder.Entity<SensorApiKey>(e =>
            {
                e.HasOne(s => s.Sensor)
                    .WithMany(z => z.SensorApiKeys)
                    .HasForeignKey(s => s.SensorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Reading
            modelBuilder.Entity<Reading>(e =>
            {
                e.HasOne(r => r.Sensor)
                    .WithMany(s => s.Readings)
                    .HasForeignKey(r => r.SensorId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(r => r.Zone)
                    .WithMany(z => z.Readings)
                    .HasForeignKey(r => r.ZoneId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(r => new { r.SensorId });
            });

            // Alert
            modelBuilder.Entity<Alert>(e =>
            {
                e.HasOne(a => a.Sensor)
                    .WithMany(s => s.Alerts)
                    .HasForeignKey(a => a.SensorId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(a => a.Batch)
                    .WithMany(b => b.Alerts)
                    .HasForeignKey(a => a.BatchId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(a => a.Zone)
                    .WithMany()
                    .HasForeignKey(a => a.ZoneId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
