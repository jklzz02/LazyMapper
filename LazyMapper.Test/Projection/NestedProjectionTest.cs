using System.Linq.Expressions;
using LazyMapper.Test.Generators;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LazyMapper.Test.Projection;

public class NestedProjectionTest
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
        });

        mapper.CreateMap<Address, AddressDto>(profile =>
        {
            profile.Bind(s => s.PostalCode, d => d.ZipCode);
        });

        mapper.CreateMap<OrderItem, OrderItemDto>();

        mapper.CreateMap<Order, OrderDto>(profile =>
        {
            profile
                .Bind(s => s.Customer, d => d.Customer)
                .Bind(s => s.ShippingAddress, d => d.ShippingAddress)
                .Bind(s => s.BillingAddress, d => d.BillingAddress)
                .Bind(s => s.Items, d => d.Items)
                .Ignore(s => s.Metadata);
        });

        return mapper;
    }

    [Theory]
    [MemberData(nameof(NestedMappingTestGenerator.ForwardMapping), MemberType = typeof(NestedMappingTestGenerator))]
    public void ProjectTo_Should_Project_Nested_Objects_Correctly(Order source, OrderDto expected)
    {
        Mapper mapper = BuildMapper();

        OrderDto result = mapper
            .ProjectTo<Order, OrderDto>(new[] { source }.AsQueryable())
            .Single();

        AssertOrder(expected, result);
    }

    [Fact]
    public void ProjectTo_Should_Project_Empty_Nested_Collections()
    {
        Order source = new()
        {
            Id = 1,
            Customer = new Customer
            {
                Id = 10,
                FirstName = "Alice",
                LastName = "Smith",
                EmailAddress = "alice@example.com",
                DateOfBirth = new DateTime(1985, 6, 20),
                IsActive = false
            },
            ShippingAddress = new Address
            {
                Street = "1 A St",
                City = "Chicago",
                PostalCode = "60601",
                CountryCode = "US"
            },
            BillingAddress = new Address
            {
                Street = "1 A St",
                City = "Chicago",
                PostalCode = "60601",
                CountryCode = "US"
            },
            Items = [],
            CreatedAt = new DateTime(2024, 5, 1),
            ShippedAt = null
        };

        Mapper mapper = BuildMapper();

            OrderDto result = mapper
            .ProjectTo<Order, OrderDto>(new[] { source }.AsQueryable())
            .Single();

        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.Null(result.ShippedAt);
        Assert.Equal(source.Customer.FirstName, result.Customer.GivenName);
        Assert.Equal(source.ShippingAddress.PostalCode, result.ShippingAddress.ZipCode);
    }

    [Fact]
    public void ProjectTo_Should_Build_Nested_MemberInit_Expression()
    {
        Mapper mapper = BuildMapper();

        IQueryable<OrderDto> query = mapper.ProjectTo<Order, OrderDto>(
            new List<Order>().AsQueryable()
        );

        MethodCallExpression selectCall = Assert.IsType<MethodCallExpression>(query.Expression, exactMatch: false);

        Assert.Equal(nameof(Queryable.Select), selectCall.Method.Name);
        Assert.Equal(typeof(Queryable), selectCall.Method.DeclaringType);

        UnaryExpression quotedSelector = Assert.IsType<UnaryExpression>(selectCall.Arguments[1]);
        Assert.Equal(ExpressionType.Quote, quotedSelector.NodeType);

        Expression<Func<Order, OrderDto>> selector =
            Assert.IsType<Expression<Func<Order, OrderDto>>>(quotedSelector.Operand, exactMatch: false);

        MemberInitExpression body = Assert.IsType<MemberInitExpression>(selector.Body);

        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.Id));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.Customer));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.ShippingAddress));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.BillingAddress));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.Items));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.CreatedAt));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(OrderDto.ShippedAt));

        MemberAssignment customerAssignment = Assert.IsType<MemberAssignment>(
            body.Bindings.Single(binding => binding.Member.Name == nameof(OrderDto.Customer))
        );

        Assert.IsType<MemberInitExpression>(customerAssignment.Expression);
    }

    [Fact]
    public void ProjectTo_Should_Be_Executable_By_Ef_Core_With_Nested_Objects()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();

        DbContextOptions<TestProjectionDbContext> options = new DbContextOptionsBuilder<TestProjectionDbContext>()
            .UseSqlite(connection)
            .Options;

        using (TestProjectionDbContext dbContext = new(options))
        {
            dbContext.Database.EnsureCreated();

            dbContext.Orders.Add(new Order
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
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "Widget",
                        Quantity = 2,
                        UnitPrice = 9.99m
                    },
                    new OrderItem
                    {
                        ProductId = 2,
                        ProductName = "Gadget",
                        Quantity = 1,
                        UnitPrice = 49.99m
                    }
                ],
                CreatedAt = new DateTime(2024, 3, 1),
                ShippedAt = new DateTime(2024, 3, 3)
            });

            dbContext.SaveChanges();
        }

        using (TestProjectionDbContext dbContext = new(options))
        {
            Mapper mapper = BuildMapper();

            OrderDto result = mapper
                .ProjectTo<Order, OrderDto>(dbContext.Orders)
                .Single();

            Assert.Equal(1, result.Id);
            Assert.Equal(new DateTime(2024, 3, 1), result.CreatedAt);
            Assert.Equal(new DateTime(2024, 3, 3), result.ShippedAt);

            Assert.Equal(10, result.Customer.Id);
            Assert.Equal("John", result.Customer.GivenName);
            Assert.Equal("Doe", result.Customer.Surname);
            Assert.Equal("john@example.com", result.Customer.Email);
            Assert.Equal(new DateTime(1990, 1, 15), result.Customer.DateOfBirth);
            Assert.True(result.Customer.IsActive);

            Assert.Equal("123 Main St", result.ShippingAddress.Street);
            Assert.Equal("New York", result.ShippingAddress.City);
            Assert.Equal("10001", result.ShippingAddress.ZipCode);
            Assert.Equal("US", result.ShippingAddress.CountryCode);

            Assert.Equal("456 Elm St", result.BillingAddress.Street);
            Assert.Equal("Boston", result.BillingAddress.City);
            Assert.Equal("02101", result.BillingAddress.ZipCode);
            Assert.Equal("US", result.BillingAddress.CountryCode);

            Assert.Equal(2, result.Items.Count);
            Assert.Contains(result.Items, item =>
                item.ProductId == 1 &&
                item.ProductName == "Widget" &&
                item.Quantity == 2 &&
                item.UnitPrice == 9.99m
            );
            Assert.Contains(result.Items, item =>
                item.ProductId == 2 &&
                item.ProductName == "Gadget" &&
                item.Quantity == 1 &&
                item.UnitPrice == 49.99m
            );
        }
    }

    [Fact]
    public void ProjectTo_Should_Throw_When_Projecting_Dictionary()
    {
        Mapper mapper = new();

        mapper.CreateMap<Order, OrderDto>();

        IQueryable<Order> source = new List<Order>().AsQueryable();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            mapper.ProjectTo<Order, OrderDto>(source)
        );

        Assert.Contains("Dictionary", exception.Message, StringComparison.Ordinal);
        Assert.Contains("not supported in projections", exception.Message, StringComparison.Ordinal);
    }

    private static void AssertOrder(OrderDto expected, OrderDto result)
    {
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

        Assert.All(expected.Items.Zip(result.Items, (expectedItem, actualItem) => (Expected: expectedItem, Actual: actualItem)),
        item =>
        {
            Assert.Equal(item.Expected.ProductId, item.Actual.ProductId);
            Assert.Equal(item.Expected.ProductName, item.Actual.ProductName);
            Assert.Equal(item.Expected.Quantity, item.Actual.Quantity);
            Assert.Equal(item.Expected.UnitPrice, item.Actual.UnitPrice);
        });
    }
}