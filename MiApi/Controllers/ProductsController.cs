using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Context;
using MiApi.Dtos;
using MiApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;

namespace MiApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
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
                CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList() // Solo obtener IDs de categorías
            });

            return Ok(productDTOs);
        }

        // GET: api/v1/products/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string name)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Name == name);

            if (product == null)
                return NotFound();

            var productDTO = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList() // Solo obtener IDs de categorías
            };

            return Ok(productDTO);
        }

        // POST: api/v1/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductDto productDto)
        {
            // Validar que el producto tenga al menos una categoría
            if (productDto.CategoryIds == null || !productDto.CategoryIds.Any())
            {
                return BadRequest(new { Error = "The product needs a category." });
            }

            // Validar unicidad del nombre del producto por categoría
            foreach (var categoryId in productDto.CategoryIds)
            {
                if (await _context.Products.AnyAsync(p => p.Name == productDto.Name && p.ProductCategories.Any(pc => pc.CategoryId == categoryId)))
                {
                    return UnprocessableEntity(new { Error = "The name is already in use" });
                }
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                ProductCategories = productDto.CategoryIds.Select(id => new ProductCategory
                {
                    CategoryId = id
                }).ToList()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Asignar el ID al DTO del producto creado para la respuesta
            productDto.Id = product.Id;

            return CreatedAtAction(nameof(GetProduct), new { name = product.Name }, productDto);
        }

        // PUT: api/v1/products/{name}
        [HttpPut("{name}")]
        public async Task<IActionResult> UpdateProduct(string name, [FromBody] ProductDto productDto)
        {
            if (name != productDto.Name)
                return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Name == name);

            if (product == null)
                return NotFound();

            // Validar que el producto tenga al menos una categoría
            if (productDto.CategoryIds == null || !productDto.CategoryIds.Any())
            {
                return BadRequest(new { Error = "The product needs a category." });
            }

            // Validar unicidad del nombre del producto por categoría
            foreach (var categoryId in productDto.CategoryIds)
            {
                if (await _context.Products.AnyAsync(p => p.Name == productDto.Name && p.Name != name && p.ProductCategories.Any(pc => pc.CategoryId == categoryId)))
                {
                    return UnprocessableEntity(new { Error = "The name is already in use" });
                }
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;

            // Limpiar categorías existentes y agregar las nuevas
            product.ProductCategories.Clear();
            product.ProductCategories = productDto.CategoryIds.Select(id => new ProductCategory
            {
                CategoryId = id
            }).ToList();

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/v1/products/{name}
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteProduct(string name)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Name == name);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
