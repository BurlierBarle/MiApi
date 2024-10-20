using MiApi.Context;
using MiApi.Dtos;
using MiApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
            .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .ToListAsync();

            var categoryDTOs = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductIds = c.ProductCategories.Select(pc => pc.ProductId).ToList()
            });

            return Ok(categoryDTOs);
        }

        // GET: api/v1/categories/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(string name)
        {
            var category = await _context.Categories
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Name == name);

            if (category == null)
                return NotFound();

            var categoryDTO = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductIds = category.ProductCategories.Select(pc => pc.ProductId).ToList()
            };

            return Ok(categoryDTO);
        }

        // POST: api/v1/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto categoryDto)
        {
            // Validate uniqueness of Name
            if (await _context.Categories.AnyAsync(c => c.Name == categoryDto.Name))
            {
                return UnprocessableEntity(new { Error = "The name is already in use." });
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            categoryDto.Name = category.Name;

            return CreatedAtAction(nameof(GetCategory), new { name = category.Name }, categoryDto);
        }

        // PUT: api/v1/categories/{name}
        [HttpPut("{name}")]
        public async Task<IActionResult> UpdateCategoryByName(string name, CategoryDto categoryDto)
        {
            // Verificar si el nombre en la URL es diferente al nombre en el DTO
            if (name != categoryDto.Name)
                return BadRequest(new { Error = "The name in the URL and the request body must match." });

            // Buscar la categoría por nombre
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);

            // Verificar si la categoría existe
            if (category == null)
                return NotFound(new { Error = "Category not found." });

            // Validar si el nuevo nombre ya está en uso por otra categoría
            if (await _context.Categories.AnyAsync(c => c.Name == categoryDto.Name && c.Id != category.Id))
            {
                return UnprocessableEntity(new { Error = "The name is already in use." });
            }

            // Actualizar los datos de la categoría
            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            _context.Entry(category).State = EntityState.Modified;

            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            // Devolver respuesta sin contenido (204)
            return NoContent();
        }

        // DELETE: api/v1/categories/{name}
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteCategory(string name)
        {
            var category = await _context.Categories
                .Include(c => c.ProductCategories)
                .FirstOrDefaultAsync(c => c.Name == name);

            if (category == null)
                return NotFound();

            if (category.ProductCategories.Any())
            {
                return UnprocessableEntity(new { Error = "Cannot delete category with assigned products." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
