using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Application.Controllers
{
    public class CreateProductPayload
    {
        public string Name  { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
    }
}
