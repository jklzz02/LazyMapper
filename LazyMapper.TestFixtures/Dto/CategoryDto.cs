namespace LazyMapper.TestFixtures.Dto;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public CategoryDto? Parent { get; set; }
    public List<CategoryDto> Children { get; set; } = null!;
    public List<ProductDto> Products { get; set; } = null!;
}