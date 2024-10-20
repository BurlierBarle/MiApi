using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Context;
using MiApi.Dtos;
using MiApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                Categories = p.ProductCategories.Select(pc => new CategoryDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Description = pc.Category.Description,
                    ProductIds = _context.Products
                        .Where(prod => prod.ProductCategories.Any(pC => pC.CategoryId == pc.Category.Id))
                        .Select(prod => prod.Id)
                        .ToList()
                }).ToList()
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
                Categories = product.ProductCategories.Select(pc => new CategoryDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Description = pc.Category.Description,
                    ProductIds = _context.Products
                        .Where(prod => prod.ProductCategories.Any(pC => pC.CategoryId == pc.Category.Id))
                        .Select(prod => prod.Id)
                        .ToList()
                }).ToList()
            };

            return Ok(productDTO);
        }

        // POST: api/v1/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto productDto)
        {
            // Validar que el producto tenga al menos una categoría
            if (productDto.CategoryIds == null || !productDto.CategoryIds.Any())
            {
                return BadRequest(new { Error = "El producto debe tener al menos una categoría." });
            }

            // Validar unicidad del nombre del producto por categoría
            foreach (var categoryId in productDto.CategoryIds)
            {
                if (await _context.Products.AnyAsync(p => p.Name == productDto.Name && p.ProductCategories.Any(pc => pc.CategoryId == categoryId)))
                {
                    return UnprocessableEntity(new { Error = "The name is already in use." });
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

            // Obtener el DTO del producto creado para la respuesta
            productDto.Id = product.Id;
            productDto.Categories = await _context.ProductCategories
                .Include(pc => pc.Category)
                .Where(pc => pc.ProductId == product.Id)
                .Select(pc => new CategoryDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Description = pc.Category.Description,
                    ProductIds = _context.Products
                        .Where(prod => prod.ProductCategories.Any(pC => pC.CategoryId == pc.Category.Id))
                        .Select(prod => prod.Id)
                        .ToList()
                }).ToListAsync();

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

            // Validar que el producto tenga al menos una categoría
            if (productDto.CategoryIds == null || !productDto.CategoryIds.Any())
            {
                return BadRequest(new { Error = "El producto debe tener al menos una categoría." });
            }

            // Validar unicidad del nombre del producto por categoría
            foreach (var categoryId in productDto.CategoryIds)
            {
                if (await _context.Products.AnyAsync(p => p.Name == productDto.Name && p.Id != id && p.ProductCategories.Any(pc => pc.CategoryId == categoryId)))
                {
                    return UnprocessableEntity(new { Error = $"El nombre del producto '{productDto.Name}' ya está en uso en la categoría con ID {categoryId}." });
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

        // DELETE: api/v1/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
