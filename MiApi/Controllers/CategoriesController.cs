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

        // GET: api/v1/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

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

            categoryDto.Id = category.Id;

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        // PUT: api/v1/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
                return BadRequest();

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            // Validate uniqueness of Name
            if (await _context.Categories.AnyAsync(c => c.Name == categoryDto.Name && c.Id != id))
            {
                return UnprocessableEntity(new { Error = "The name is already in use." });
            }

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            _context.Entry(category).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/v1/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ProductCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

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
