using MiApi.Context;
using MiApi.Dtos;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MiApi.Validators
{
    public class CategoryValidator : AbstractValidator<CategoryDto>
    {
        private readonly AppDbContext _context;

        public CategoryValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MustAsync(BeUniqueName).WithMessage("The name is already in use.");

            // Description is optional, so no need to validate
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _context.Categories.AnyAsync(c => c.Name == name, cancellationToken);
        }
    }
}
