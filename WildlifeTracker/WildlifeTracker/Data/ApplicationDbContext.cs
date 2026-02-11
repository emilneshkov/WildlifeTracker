using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WildlifeTracker.Models;
using WildlifeTracker.Models.Identity;

namespace WildlifeTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Municipality> Municipalities => Set<Municipality>();
        public DbSet<Settlement> Settlements => Set<Settlement>();
        public DbSet<Species> Species => Set<Species>();
        public DbSet<InitialPopulation> InitialPopulations => Set<InitialPopulation>();
        public DbSet<PopulationChange> PopulationChanges => Set<PopulationChange>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Уникална община по име 
            builder.Entity<Municipality>()
                .HasIndex(m => m.Name)
                .IsUnique();

            // Уникално населено място в рамките на община
            builder.Entity<Settlement>()
                .HasIndex(s => new { s.MunicipalityId, s.Name })
                .IsUnique();

            // Уникален вид по име
            builder.Entity<Species>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // 1 начална бройка за (Settlement, Species)
            builder.Entity<InitialPopulation>()
                .HasIndex(ip => new { ip.SettlementId, ip.SpeciesId })
                .IsUnique();

            // 1 промяна за (Settlement, Species, Year) — “веднъж годишно”
            builder.Entity<PopulationChange>()
                .HasIndex(pc => new { pc.SettlementId, pc.SpeciesId, pc.Year })
                .IsUnique();

            // Забрани каскадно триене (тъй като данните са “предварително зададени”)
            builder.Entity<Settlement>()
                .HasOne(s => s.Municipality)
                .WithMany(m => m.Settlements)
                .HasForeignKey(s => s.MunicipalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InitialPopulation>()
                .HasOne(ip => ip.Settlement)
                .WithMany(s => s.InitialPopulations)
                .HasForeignKey(ip => ip.SettlementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InitialPopulation>()
                .HasOne(ip => ip.Species)
                .WithMany(sp => sp.InitialPopulations)
                .HasForeignKey(ip => ip.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PopulationChange>()
                .HasOne(pc => pc.Settlement)
                .WithMany(s => s.PopulationChanges)
                .HasForeignKey(pc => pc.SettlementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PopulationChange>()
                .HasOne(pc => pc.Species)
                .WithMany(sp => sp.PopulationChanges)
                .HasForeignKey(pc => pc.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PopulationChange>()
                .HasOne(pc => pc.EnteredByUser)
                .WithMany()
                .HasForeignKey(pc => pc.EnteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser -> Settlement (nullable)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Settlement)
                .WithMany()
                .HasForeignKey(u => u.SettlementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
