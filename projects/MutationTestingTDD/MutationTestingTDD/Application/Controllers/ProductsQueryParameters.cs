using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Application.Controllers
{
    public class ProductsQueryParameters
    {
        public ProductCategory? Category { get; set; }

        public decimal? MaxPrice { get; set; }

        public bool? IsOnSale { get; set; }
    }
}
