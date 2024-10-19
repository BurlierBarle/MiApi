namespace MiApi.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<int> CategoryIds { get; set; }
    }
}
