namespace LazyMapper.TestFixtures.Classes;

public class Warehouse
{
    public int Id { get; set; }
    public string Location { get; set; } = "";
    public List<List<Product>> Shelves { get; set; } = [[]];
    public Dictionary<string, List<Order>> OrdersByRegion { get; set; } = new ();
}