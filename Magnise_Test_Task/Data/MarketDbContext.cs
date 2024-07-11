using Magnise_Test_Task.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magnise_Test_Task.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options)
        : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<AssetProviderMapping> AssetProviderMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.Property(e => e.Currency).HasMaxLength(50);
                entity.Property(e => e.Symbol).HasMaxLength(50);
                entity.Property(e => e.Exchange).HasMaxLength(50);
                entity.Property(e => e.BaseCurrency).HasMaxLength(50);
                entity.Property(e => e.Kind).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(100);

                entity.HasMany(a => a.Mappings)
                    .WithOne(m => m.Asset)
                    .HasForeignKey(m => m.AssetId);
            });

            modelBuilder.Entity<AssetProviderMapping>(entity =>
            {
                entity.Property(e => e.Provider).HasMaxLength(50);
                entity.Property(e => e.Symbol).HasMaxLength(50);
                entity.Property(e => e.Exchange).HasMaxLength(50);

                entity.HasOne(e => e.Asset)
                    .WithMany(a => a.Mappings)
                    .HasForeignKey(e => e.AssetId)
                    .IsRequired();
            });

            modelBuilder.Entity<Price>(entity =>
            {
                entity.Property(e => e.Value).HasPrecision(12, 6);

                entity.HasOne(p => p.AssetProviderMapping)
                    .WithOne(m => m.Price)
                    .HasForeignKey<Price>(p => p.AssetProviderMappingId)
                    .IsRequired();
            });

        }
    }

}
