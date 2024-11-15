using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Application.Controllers
{
    public class ProductsQueryParameters
    {
        public string? SearchText { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
    }
}
