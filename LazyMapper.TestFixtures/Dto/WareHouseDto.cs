namespace LazyMapper.TestFixtures.Dto;

public class WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<List<ProductDto>> Shelves { get; set; } = [[]];
    public Dictionary<string, List<OrderDto>> OrdersByRegion { get; set; } = new();
}