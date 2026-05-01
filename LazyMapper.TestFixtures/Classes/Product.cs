namespace LazyMapper.TestFixtures.Classes;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}