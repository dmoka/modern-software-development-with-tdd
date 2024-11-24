﻿using Microsoft.EntityFrameworkCore;
using RefactoringLegacyCode.Features;

namespace RefactoringLegacyCode.Data
{

    public class Product
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public void DecreaseQuantity(int quantity)
        {
            Quantity -= quantity;
        }
    }

    public class WarehousingDbContext : DbContext
    {
        public WarehousingDbContext(DbContextOptions<WarehousingDbContext> options) : base(options)
        {
        }

        public DbSet<OrderDetails> Orders { get; set; }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetails>().ToTable("Orders").HasKey(o => o.Id);
            modelBuilder.Entity<Product>().ToTable("Products").HasKey(o => o.Id);

            // Configure enum-to-string conversion for Status
            modelBuilder.Entity<OrderDetails>().Property(o => o.DeliveryType)
                .HasConversion<string>();

            // Configure enum-to-string conversion for Status
           /* modelBuilder.Entity<OrderDetails>().Property(o => o.Status)
                .HasConversion<string>();*/
        }
    }
}