using System.Collections.Generic;
using System.Reflection.Emit;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using VerticalSlicingArchitecture.Features;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Database
{

    public class WarehousingDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<StockLevel> StockLevels { get; set; }

        public WarehousingDbContext(DbContextOptions<WarehousingDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(p =>
            {
                p.HasKey(p => p.Id);
                p.Property(p => p.Name).IsRequired();
                p.Property(p => p.Description).IsRequired();
                p.Property(p => p.Price).IsRequired();
            });

            modelBuilder.Entity<StockLevel>(p =>
            {
                p.HasKey(p => p.Id);
                p.HasIndex(p => p.ProductId).IsUnique();
                p.Property(p => p.Quantity).IsRequired();
            });
        }
    }

    public class Product
    {
        public Product(string name, string description, decimal price)
        {
            Id = Guid.NewGuid();;
            Name = name;
            Description = description;
            Price = price;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }
        public decimal Price { get;}
        public StockLevel StockLevel { get; set; }

    }
}

public class StockLevel
{
    public Guid Id { get; }
    public Guid ProductId { get; }
    public int Quantity { get; private set; }

    public StockLevel(Guid productId, int quantity)
    {
        Id = Guid.NewGuid();

        ProductId = productId;
        Quantity = quantity;
    }
}
