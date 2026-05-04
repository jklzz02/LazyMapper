namespace LazyMapper.TestFixtures.Models;

public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    public List<OrderItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}