using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Data
{
    public class ProductEntityConfiguration : EntityConfigurationBase<Product>
    {
        private readonly EnumToStringConverter<ProductCategory> productCategoryToStringConverter = new EnumToStringConverter<ProductCategory>();

        public override void Configure(EntityTypeBuilder<Product> builder)
        {

            builder.HasKey(s => s.Id);

            // Configure the Name property
            builder.Property(s => s.Name)
                .HasMaxLength(100) // Maximum length of 100 characters
                .IsUnicode(true) // Ensure Unicode (equivalent to nvarchar)
                .IsRequired();

            // Configure the Category property with default value and conversion
            builder.Property(e => e.Category)
                .HasMaxLength(20) // Maximum length of 20 characters
                .IsUnicode(true) // Ensure Unicode
                .HasConversion(productCategoryToStringConverter);

            // Configure the Price property
            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(12, 2); // Decimal precision of 12, scale 2

            // Configure the SaleState property
            builder.Property(p => p.SaleState)
                .HasMaxLength(50) // Assuming a reasonable length for SaleState
                .IsUnicode(false) // Assuming SaleState is not Unicode (varchar)
                .IsRequired();
        }
    }
}
