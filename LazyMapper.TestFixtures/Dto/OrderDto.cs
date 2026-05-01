namespace LazyMapper.TestFixtures.Dto;

public class OrderDto
{
    public int Id { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public AddressDto ShippingAddress { get; set; } = new();
    public AddressDto BillingAddress { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}