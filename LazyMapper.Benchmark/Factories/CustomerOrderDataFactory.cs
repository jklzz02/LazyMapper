using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark.Factories;

public static class BenchmarkDataFactory
{
    public static Customer CreateCustomer(int id)
    {
        return new Customer
        {
            Id = id,
            FirstName = $"FirstName {id}",
            LastName = $"LastName {id}",
            EmailAddress = $"customer{id}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1).AddDays(id),
            IsActive = id % 2 == 0
        };
    }

    public static List<Customer> CreateCustomers(int count)
    {
        return Enumerable
            .Range(1, count)
            .Select(CreateCustomer)
            .ToList();
    }

    public static Address CreateAddress(int id)
    {
        return new Address
        {
            Street = $"{id} Benchmark Street",
            City = $"City {id}",
            PostalCode = $"ZIP-{id:00000}",
            CountryCode = "IT"
        };
    }

    public static OrderItem CreateOrderItem(int id)
    {
        return new OrderItem
        {
            ProductId = id,
            ProductName = $"Product {id}",
            Quantity = id % 10 + 1,
            UnitPrice = 10.50m + id
        };
    }

    public static List<OrderItem> CreateOrderItems(int count)
    {
        return Enumerable
            .Range(1, count)
            .Select(CreateOrderItem)
            .ToList();
    }

    public static Order CreateOrder(int id, int itemCount)
    {
        return new Order
        {
            Id = id,
            Customer = CreateCustomer(id),
            ShippingAddress = CreateAddress(id),
            BillingAddress = CreateAddress(id + 10_000),
            Items = CreateOrderItems(itemCount),
            CreatedAt = DateTime.UtcNow.AddDays(-id),
            ShippedAt = id % 2 == 0 ? DateTime.UtcNow.AddDays(-id + 1) : null,
            Metadata = new Dictionary<string, string>
            {
                ["Source"] = "Benchmark",
                ["OrderNumber"] = $"ORD-{id:000000}",
                ["Scenario"] = "NestedMapping"
            }
        };
    }

    public static List<Order> CreateOrders(int count, int itemCount)
    {
        return Enumerable
            .Range(1, count)
            .Select(id => CreateOrder(id, itemCount))
            .ToList();
    }

    public static Product CreateProduct(int id)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = $"Product {id}",
            Price = 50m + id,
            DiscountedPrice = id % 2 == 0 ? 45m + id : null,
            StockQuantity = id % 3 == 0 ? null : id * 10,
            IsAvailable = id % 2 == 0,
            Tags =
            [
                "benchmark",
                $"product-{id}",
                id % 2 == 0 ? "even" : "odd"
            ]
        };
    }

    public static List<Product> CreateProducts(int count)
    {
        return Enumerable
            .Range(1, count)
            .Select(CreateProduct)
            .ToList();
    }
}
