using LazyMapper.TestFixtures.Classes;
using LazyMapper.TestFixtures.Dto;

namespace LazyMapper.Test.Generators;

public class NestedMappingTestGenerator
{
    public static TheoryData<Order, OrderDto> ForwardMapping() => new()
    {
        {
            new Order
            {
                Id = 1,
                Customer = new Customer
                {
                    Id = 10,
                    FirstName = "John",
                    LastName = "Doe",
                    EmailAddress = "john@example.com",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    IsActive = true
                },
                ShippingAddress = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    PostalCode = "10001",
                    CountryCode = "US"
                },
                BillingAddress = new Address
                {
                    Street = "456 Elm St",
                    City = "Boston",
                    PostalCode = "02101",
                    CountryCode = "US"
                },
                Items =
                [
                    new OrderItem { ProductId = 1, ProductName = "Widget", Quantity = 2, UnitPrice = 9.99m },
                    new OrderItem { ProductId = 2, ProductName = "Gadget", Quantity = 1, UnitPrice = 49.99m }
                ],
                CreatedAt = new DateTime(2024, 3, 1),
                ShippedAt = new DateTime(2024, 3, 3),
                Metadata = new Dictionary<string, string> { ["note"] = "fragile", ["priority"] = "high" }
            },
            new OrderDto
            {
                Id = 1,
                Customer = new CustomerDto
                {
                    Id = 10,
                    GivenName = "John",
                    Surname = "Doe",
                    Email = "john@example.com",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    IsActive = true
                },
                ShippingAddress = new AddressDto
                {
                    Street = "123 Main St",
                    City = "New York",
                    ZipCode = "10001",
                    CountryCode = "US"
                },
                BillingAddress = new AddressDto
                {
                    Street = "456 Elm St",
                    City = "Boston",
                    ZipCode = "02101",
                    CountryCode = "US"
                },
                Items =
                [
                    new OrderItemDto { ProductId = 1, ProductName = "Widget", Quantity = 2, UnitPrice = 9.99m },
                    new OrderItemDto { ProductId = 2, ProductName = "Gadget", Quantity = 1, UnitPrice = 49.99m }
                ],
                CreatedAt = new DateTime(2024, 3, 1),
                ShippedAt = new DateTime(2024, 3, 3),
                Metadata = new Dictionary<string, string> { ["note"] = "fragile", ["priority"] = "high" }
            }
        },
        
        {
            new Order
            {
                Id = 2,
                Customer = new Customer
                {
                    Id = 20,
                    FirstName = "Alice",
                    LastName = "Smith",
                    EmailAddress = "alice@example.com",
                    DateOfBirth = new DateTime(1985, 6, 20),
                    IsActive = false
                },
                ShippingAddress = new Address { Street = "1 A St", City = "Chicago", PostalCode = "60601", CountryCode = "US" },
                BillingAddress = new Address { Street = "1 A St", City = "Chicago", PostalCode = "60601", CountryCode = "US" },
                Items = [],
                CreatedAt = new DateTime(2024, 5, 1),
                ShippedAt = null,
                Metadata = new Dictionary<string, string>()
            },
            new OrderDto
            {
                Id = 2,
                Customer = new CustomerDto
                {
                    Id = 20,
                    GivenName = "Alice",
                    Surname = "Smith",
                    Email = "alice@example.com",
                    DateOfBirth = new DateTime(1985, 6, 20),
                    IsActive = false
                },
                ShippingAddress = new AddressDto { Street = "1 A St", City = "Chicago", ZipCode = "60601", CountryCode = "US" },
                BillingAddress = new AddressDto { Street = "1 A St", City = "Chicago", ZipCode = "60601", CountryCode = "US" },
                Items = [],
                CreatedAt = new DateTime(2024, 5, 1),
                ShippedAt = null,
                Metadata = new Dictionary<string, string>()
            }
        }
    };

    public static TheoryData<Order> RoundTrip() =>
    [
        new Order
        {
            Id = 1,
            Customer = new Customer
            {
                Id = 10,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            },
            ShippingAddress = new Address { Street = "123 Main St", City = "New York", PostalCode = "10001", CountryCode = "US" },
            BillingAddress = new Address { Street = "456 Elm St", City = "Boston", PostalCode = "02101", CountryCode = "US" },
            Items =
            [
                new OrderItem { ProductId = 1, ProductName = "Widget", Quantity = 2, UnitPrice = 9.99m }
            ],
            CreatedAt = new DateTime(2024, 3, 1),
            ShippedAt = new DateTime(2024, 3, 3),
            Metadata = new Dictionary<string, string> { ["note"] = "fragile" }
        },
        new Order
        {
            Id = 2,
            Customer = new Customer
            {
                Id = 20,
                FirstName = "Alice",
                LastName = "Smith",
                EmailAddress = "alice@example.com"
            },
            ShippingAddress = new Address { Street = "1 A St", City = "Chicago", PostalCode = "60601", CountryCode = "US" },
            BillingAddress = new Address { Street = "1 A St", City = "Chicago", PostalCode = "60601", CountryCode = "US" },
            Items = [],
            CreatedAt = DateTime.MinValue,
            ShippedAt = null,
            Metadata = new Dictionary<string, string>()
        }
    ];
}