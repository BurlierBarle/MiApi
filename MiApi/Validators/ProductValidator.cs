using FluentValidation;
using MiApi.Context;
using MiApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MiApi.Validators
{
    public class ProductValidator : AbstractValidator<ProductDto>
    {
        private readonly AppDbContext _context;

        public ProductValidator(AppDbContext context)
        {
            _context = context;

            // Reglas de validación
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.")
                .MustAsync(BeUniqueName).WithMessage("El nombre ya está en uso.");
        }


        // Validación para verificar si el nombre es único
        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _context.Products.AnyAsync(p => p.Name == name, cancellationToken);
        }

        // Validación para verificar que las categorías existen en la base de datos
        private async Task<bool> ExistInDatabase(List<int> categoryIds, CancellationToken cancellationToken)
        {
            var existingCategories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            // Todos los IDs enviados deben existir en la base de datos
            return categoryIds.All(id => existingCategories.Contains(id));
        }
    }
}
