using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Data
{
    public class InventoryLevelConfigurator : EntityConfigurationBase<StockLevel>
    {


        public override void Configure(EntityTypeBuilder<StockLevel> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(p => p.ProductId)
                .IsRequired();

            builder.Property(p => p.Count)
                .IsRequired();
        }
    }
}
