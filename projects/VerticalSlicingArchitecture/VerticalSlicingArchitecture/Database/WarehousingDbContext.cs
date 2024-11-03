using static VerticalSlicingArchitecture.Features.Product.CreateProduct;
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

        public DbSet<Product> Products { get; set; }

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
            });
        }
    }

}
