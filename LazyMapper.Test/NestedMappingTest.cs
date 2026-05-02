using System.Diagnostics;
using LazyMapper.Lib;
using LazyMapper.Test.Generators;
using LazyMapper.TestFixtures.Classes;
using LazyMapper.TestFixtures.Dto;

namespace LazyMapper.Test;

public class NestedMappingTest
{
    private static Mapper BuildMapper()
    {
        Mapper mapper = new();

        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        }).ReverseMap();

        mapper.CreateMap<Address, AddressDto>(profile =>
        {
            profile.Bind(s => s.PostalCode, d => d.ZipCode);
        }).ReverseMap();

        mapper.CreateMap<OrderItem, OrderItemDto>().ReverseMap();

        mapper.CreateMap<Order, OrderDto>(profile =>
        {
            profile
                .Bind(s => s.Customer, d => d.Customer)
                .Bind(s => s.ShippingAddress, d => d.ShippingAddress)
                .Bind(s => s.BillingAddress, d => d.BillingAddress)
                .Bind(s => s.Items, d => d.Items);
        }).ReverseMap();

        return mapper;
    }

    [Theory]
    [MemberData(nameof(NestedMappingTestGenerator.ForwardMapping), MemberType = typeof(NestedMappingTestGenerator))]
    public void Nested_Classes_Should_Map_Correctly(Order source, OrderDto expected)
    {
        Mapper mapper = BuildMapper();
        OrderDto result = mapper.Map<Order, OrderDto>(source);

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.CreatedAt, result.CreatedAt);
        Assert.Equal(expected.ShippedAt, result.ShippedAt);

        Assert.Equal(expected.Customer.Id, result.Customer.Id);
        Assert.Equal(expected.Customer.GivenName, result.Customer.GivenName);
        Assert.Equal(expected.Customer.Surname, result.Customer.Surname);
        Assert.Equal(expected.Customer.Email, result.Customer.Email);
        Assert.Equal(expected.Customer.DateOfBirth, result.Customer.DateOfBirth);
        Assert.Equal(expected.Customer.IsActive, result.Customer.IsActive);

        Assert.Equal(expected.ShippingAddress.Street, result.ShippingAddress.Street);
        Assert.Equal(expected.ShippingAddress.City, result.ShippingAddress.City);
        Assert.Equal(expected.ShippingAddress.ZipCode, result.ShippingAddress.ZipCode);
        Assert.Equal(expected.ShippingAddress.CountryCode, result.ShippingAddress.CountryCode);

        Assert.Equal(expected.BillingAddress.Street, result.BillingAddress.Street);
        Assert.Equal(expected.BillingAddress.City, result.BillingAddress.City);
        Assert.Equal(expected.BillingAddress.ZipCode, result.BillingAddress.ZipCode);
        Assert.Equal(expected.BillingAddress.CountryCode, result.BillingAddress.CountryCode);

        Assert.Equal(expected.Items.Count, result.Items.Count);
        Assert.All(expected.Items.Zip(
            result.Items,
            (e, a) => (Expected: e, Actual: a)),
            item =>
            {
                Assert.Equal(item.Expected.ProductId, item.Actual.ProductId);
                Assert.Equal(item.Expected.ProductName, item.Actual.ProductName);
                Assert.Equal(item.Expected.Quantity, item.Actual.Quantity);
                Assert.Equal(item.Expected.UnitPrice, item.Actual.UnitPrice);
            });

        Assert.Equal(expected.Metadata, result.Metadata);
    }

    [Theory]
    [MemberData(nameof(NestedMappingTestGenerator.RoundTrip), MemberType = typeof(NestedMappingTestGenerator))]
    public void Nested_Classes_RoundTrip_Should_Return_Same_Object(Order source)
    {
        Mapper mapper = BuildMapper();
        OrderDto dto = mapper.Map<Order, OrderDto>(source);
        Order result = mapper.Map<OrderDto, Order>(dto);

        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.CreatedAt, result.CreatedAt);
        Assert.Equal(source.ShippedAt, result.ShippedAt);

        Assert.Equal(source.Customer.Id, result.Customer.Id);
        Assert.Equal(source.Customer.FirstName, result.Customer.FirstName);
        Assert.Equal(source.Customer.LastName, result.Customer.LastName);
        Assert.Equal(source.Customer.EmailAddress, result.Customer.EmailAddress);
        Assert.Equal(source.Customer.DateOfBirth, result.Customer.DateOfBirth);
        Assert.Equal(source.Customer.IsActive, result.Customer.IsActive);

        Assert.Equal(source.ShippingAddress.Street, result.ShippingAddress.Street);
        Assert.Equal(source.ShippingAddress.City, result.ShippingAddress.City);
        Assert.Equal(source.ShippingAddress.PostalCode, result.ShippingAddress.PostalCode);
        Assert.Equal(source.ShippingAddress.CountryCode, result.ShippingAddress.CountryCode);

        Assert.Equal(source.BillingAddress.Street, result.BillingAddress.Street);
        Assert.Equal(source.BillingAddress.City, result.BillingAddress.City);
        Assert.Equal(source.BillingAddress.PostalCode, result.BillingAddress.PostalCode);
        Assert.Equal(source.BillingAddress.CountryCode, result.BillingAddress.CountryCode);

        Assert.Equal(source.Items.Count, result.Items.Count);
        Assert.All(source.Items.Zip(
            result.Items,
            (e, a) => (Expected: e, Actual: a)),
            item =>
            {
                Assert.Equal(item.Expected.ProductId, item.Actual.ProductId);
                Assert.Equal(item.Expected.ProductName, item.Actual.ProductName);
                Assert.Equal(item.Expected.Quantity, item.Actual.Quantity);
                Assert.Equal(item.Expected.UnitPrice, item.Actual.UnitPrice);
            });
    }

    [Fact]
    public void Null_Nested_Object_Should_Map_To_Null()
    {
        Mapper mapper = BuildMapper();
        Order source = new() { Id = 1, Customer = null!, Items = [] };

        OrderDto result = mapper.Map<Order, OrderDto>(source);

        Assert.Null(result.Customer);
    }

    [Fact]
    public void Null_Collection_Should_Map_To_Null()
    {
        Mapper mapper = BuildMapper();
        Order source = new() { Id = 1, Items = null! };

        OrderDto result = mapper.Map<Order, OrderDto>(source);

        Assert.Null(result.Items);
    }

    [Fact]
    public void Empty_Collection_Should_Map_To_Empty_Not_Null()
    {
        Mapper mapper = BuildMapper();
        Order source = new() { Id = 1, Items = [] };

        OrderDto result = mapper.Map<Order, OrderDto>(source);

        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void Null_Dictionary_Should_Map_To_Null()
    {
        Mapper mapper = BuildMapper();
        Order source = new() { Id = 1, Items = [], Metadata = null! };

        OrderDto result = mapper.Map<Order, OrderDto>(source);

        Assert.Null(result.Metadata);
    }

    [Fact]
    public void Same_Type_Two_Properties_Should_Be_Reference_Isolated()
    {
        Mapper mapper = BuildMapper();
        Order source = new()
        {
            Id = 1,
            ShippingAddress = new Address { Street = "123 Main St", City = "New York", PostalCode = "10001", CountryCode = "US" },
            BillingAddress = new Address { Street = "456 Elm St", City = "Boston", PostalCode = "02101", CountryCode = "US" },
            Items = []
        };

        OrderDto result = mapper.Map<Order, OrderDto>(source);
        result.ShippingAddress.Street = "MUTATED";

        Assert.NotEqual(result.ShippingAddress.Street, result.BillingAddress.Street);
    }

    [Fact]
    public void Dictionary_Should_Not_Share_Reference_With_Source()
    {
        Mapper mapper = BuildMapper();
        Order source = new()
        {
            Id = 1,
            Items = [],
            Metadata = new Dictionary<string, string> { ["key"] = "original" }
        };

        OrderDto result = mapper.Map<Order, OrderDto>(source);
        result.Metadata["key"] = "MUTATED";

        Assert.Equal("original", source.Metadata["key"]);
    }

    [Fact]
    public void Nested_Collection_Should_Not_Share_Reference_With_Source()
    {
        Mapper mapper = BuildMapper();
        Order source = new()
        {
            Id = 1,
            Items = [new OrderItem { ProductId = 1, ProductName = "Widget", Quantity = 1, UnitPrice = 9.99m }]
        };

        OrderDto result = mapper.Map<Order, OrderDto>(source);
        result.Items[0].ProductName = "MUTATED";

        Assert.Equal("Widget", source.Items[0].ProductName);
    }

    [Fact]
    public void Deep_Nested_Category_Tree_Should_Not_Stack_Overflow()
    {
        Mapper mapper = new();
        mapper.CreateMap<Category, CategoryDto>().ReverseMap();
        mapper.CreateMap<Product, ProductDto>().ReverseMap();

        Category root = new() { Id = 1, Name = "Root", Children = [], Products = [] };
        Category current = root;
        for (int i = 2; i <= 1000; i++)
        {
            Category child = new() { Id = i, Name = $"Level {i}", Children = [], Products = [] };
            current.Children.Add(child);
            child.Parent = current;
            current = child;
        }

        var ex = Record.Exception(() => mapper.Map<Category, CategoryDto>(root));

        Assert.Null(ex);
    }

    [Fact]
    public void Circular_Reference_Should_Not_Stack_Overflow()
    {
        Mapper mapper = new();
        mapper.CreateMap<Category, CategoryDto>().ReverseMap();
        mapper.CreateMap<Product, ProductDto>().ReverseMap();

        Category parent = new() { Id = 1, Name = "Parent", Children = [], Products = [] };
        Category child = new() { Id = 2, Name = "Child", Children = [], Products = [] };
        parent.Children.Add(child);
        child.Parent = parent;
        child.Children.Add(parent);

        var ex = Record.Exception(() => mapper.Map<Category, CategoryDto>(parent));

        Assert.Null(ex);
    }

    [Fact]
    public void Mapping_Large_Collection_Should_Scale_Decently()
    {
        Mapper mapper = BuildMapper();

        List<Order> BuildOrders(int count) => Enumerable.Range(1, count).Select(i => new Order
        {
            Id = i,
            Customer = new Customer { Id = i, FirstName = "John", LastName = "Doe", EmailAddress = "j@j.com" },
            ShippingAddress = new Address { Street = "St", City = "NY", PostalCode = "100", CountryCode = "US" },
            BillingAddress = new Address { Street = "St", City = "NY", PostalCode = "100", CountryCode = "US" },
            Items = [new OrderItem { ProductId = 1, ProductName = "W", Quantity = 1, UnitPrice = 1m }],
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string> { ["k"] = "v" }
        }).ToList();

        IEnumerable<Order> smallOrders = BuildOrders(100).ToList();
        var sw = Stopwatch.StartNew();
        mapper.Map<Order, OrderDto>(smallOrders);
        long smallMs = sw.ElapsedMilliseconds;

        IEnumerable<Order> largeOrders = BuildOrders(10000).ToList();
        sw.Restart();
        mapper.Map<Order, OrderDto>(largeOrders);
        long largeMs = sw.ElapsedMilliseconds;
        
        
        IEnumerable<Order> hugeOrders = BuildOrders(100_000).ToList();
        sw.Restart();
        mapper.Map<Order, OrderDto>(hugeOrders);
        long hugeMs = sw.ElapsedMilliseconds;

        Assert.True(largeMs < Math.Max(smallMs, 1) * 10,
            $"Mapping scaled poorly: 100 items={smallMs}ms, 10000 items={largeMs}ms");
        
        Assert.True(hugeMs < Math.Max(largeMs, 1) * 10,
            $"Mapping scaled poorly: 100 items={largeMs}ms, 10000 items={hugeMs}ms");
    }

    [Fact]
    public void Mapping_Deep_Tree_Should_Complete_Within_Acceptable_Time()
    {
        Mapper mapper = new();
        mapper.CreateMap<Category, CategoryDto>().ReverseMap();
        mapper.CreateMap<Product, ProductDto>().ReverseMap();

        Category root = new() { Id = 1, Name = "Root", Children = [], Products = [] };
        Category current = root;
        for (int i = 2; i <= 500; i++)
        {
            Category child = new() { Id = i, Name = $"Level {i}", Children = [], Products = [] };
            current.Children.Add(child);
            current = child;
        }

        var sw = Stopwatch.StartNew();
        mapper.Map<Category, CategoryDto>(root);
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 100,
            $"Deep tree mapping took too long: {sw.ElapsedMilliseconds}ms");
    }
}