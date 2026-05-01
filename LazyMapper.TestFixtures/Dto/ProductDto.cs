namespace LazyMapper.TestFixtures.Dto;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Tags { get; set; } = [];
}