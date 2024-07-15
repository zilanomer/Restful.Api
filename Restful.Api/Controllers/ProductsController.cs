using Microsoft.AspNetCore.Mvc;
using Restful.Api.Models;
using Restful.Api.Repositories;
using Microsoft.AspNetCore.JsonPatch;


namespace Restful.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]  
        public IActionResult Get() // tüm ürünleri döndürmek için kullanýrýz.
        {
            var products = _repository.GetAll();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = _repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product); //200 
        }

        [HttpPost]
        public IActionResult Post([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _repository.Add(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product); //201 
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProduct = _repository.GetById(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            product.Id = id;
            _repository.Update(product);
            return NoContent();  // 204 
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            _repository.Delete(id);
            return NoContent();
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string name)
        {
            var products = _repository.GetAll().Where(p => p.Name.Contains(name));
            return Ok(products);
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] JsonPatchDocument<Product> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var product = _repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            patchDoc.ApplyTo(product, ModelState);

            if (!TryValidateModel(product))
            {
                return ValidationProblem(ModelState);
            }

            _repository.Update(product);
            return NoContent();
        }

        [HttpGet("list")]
        public IActionResult List([FromQuery] string name, [FromQuery] string sortBy) 
        {
            var products = _repository.GetAll();

            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "name")
                {
                    products = products.OrderBy(p => p.Name);
                }
                else if (sortBy == "price")
                {
                    products = products.OrderBy(p => p.Price);
                }
            }

            return Ok(products);
        }

    }
}
