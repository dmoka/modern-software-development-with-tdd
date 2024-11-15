using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MutationTestingTDD.Domain;

namespace MutationTestingTDD.Application.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductsFinder _productsFinder;

        public ProductsController(IUnitOfWork unitOfWork, IProductsFinder productsFinder)
        {
            _unitOfWork = unitOfWork;
            _productsFinder = productsFinder;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _unitOfWork.Products.GetAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(new ProductViewModel()
            {
                Id = product.Id, 
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductsQueryParameters queryParameters)
        {
            if (queryParameters.SearchText.IsNullOrEmpty())
            {
                return BadRequest("The search text must be specified");
            }

            var products = _productsFinder.Find(queryParameters).Select(p => new ProductViewModel()
            {
                Id =  p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price
            });

            return Ok(products);
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductPayload productPayload)
        {

            if (_unitOfWork.Products.Exists(productPayload.Name))
            {
                return StatusCode(409);
            }

            var product = new Product(productPayload.Name, productPayload.Description, productPayload.Price);

            var storedProduct = _unitOfWork.Products.Create(product);

            await _unitOfWork.CommitAsync();

            return CreatedAtAction("GetProduct", new { id = storedProduct.Id }, storedProduct);
        }


        [HttpPost]
        [Route("{id}/pick")]
        public async Task<IActionResult> PickProduct(Guid id, PickPayload payload)
        {
            try
            {
                var product = await _unitOfWork.Products.GetAsync(id);
                product.Pick(payload.Count);
                await _unitOfWork.CommitAsync();
            }
            catch (ApplicationException e)
            {
                return BadRequest(e.Message);
            }


            return StatusCode(204);
        }

       /* [HttpPost]
        [Route("{id}/unpick")]
        public async Task<IActionResult> UnpickProduct(Guid id, PickPayload payload)
        {
            try
            {
                var product = await _unitOfWork.Products.GetAsync(id);
                product.Unpick(payload.Count);
                await _unitOfWork.CommitAsync();
            }
            catch (ApplicationException)
            {
                return BadRequest();
            }


            return StatusCode(204);
        }*/
    }
}
