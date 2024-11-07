using static VerticalSlicingArchitecture.Features.Product.Create;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Entities;

namespace VerticalSlicingArchitecture.Database
{

    public class WarehousingDbContext : DbContext
    {
        public WarehousingDbContext(DbContextOptions<WarehousingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<StockLevel> StockLevels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2);

               /* entity.HasOne<StockLevel>()
                    .WithOne()
                    .HasForeignKey<StockLevel>(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);*/
            });

            modelBuilder.Entity<StockLevel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProductId).IsUnique();
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
            });
        }
    }

}
