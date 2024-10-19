using System.Collections.Generic;

namespace MiApi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
