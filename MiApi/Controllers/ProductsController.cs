using MiApi.Context;
using MiApi.Dtos;
using MiApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiApi.Controllers
{
    [Route("api/v1/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
                .ToListAsync();

            var productDTOs = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList()
            });

            return Ok(productDTOs);
        }

        // GET: api/v1/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var productDTO = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList()
            };

            return Ok(productDTO);
        }

        // POST: api/v1/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto productDto)
        {
            // Validate uniqueness of Name
            if (await _context.Products.AnyAsync(p => p.Name == productDto.Name))
            {
                return UnprocessableEntity(new { Error = "The name is already in use." });
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Handle many-to-many relationship
            foreach (var categoryId in productDto.CategoryIds)
            {
                var productCategory = new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId
                };
                _context.ProductCategories.Add(productCategory);
            }
            await _context.SaveChangesAsync();

            productDto.Id = product.Id;

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        // PUT: api/v1/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Validate uniqueness of Name
            if (await _context.Products.AnyAsync(p => p.Name == productDto.Name && p.Id != id))
            {
                return UnprocessableEntity(new { Error = "The name is already in use." });
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;

            // Update many-to-many relationship
            var existingCategories = product.ProductCategories.Select(pc => pc.CategoryId).ToList();
            var newCategories = productDto.CategoryIds;

            // Remove categories not in newCategories
            foreach (var categoryId in existingCategories.Except(newCategories))
            {
                var productCategory = await _context.ProductCategories
                    .FirstOrDefaultAsync(pc => pc.ProductId == id && pc.CategoryId == categoryId);
                _context.ProductCategories.Remove(productCategory);
            }

            // Add new categories
            foreach (var categoryId in newCategories.Except(existingCategories))
            {
                var productCategory = new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId
                };
                _context.ProductCategories.Add(productCategory);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/v1/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Remove related categories
            var productCategories = _context.ProductCategories.Where(pc => pc.ProductId == id);
            _context.ProductCategories.RemoveRange(productCategories);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
