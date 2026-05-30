using System.Linq.Expressions;
using LazyMapper.Test.Generators;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LazyMapper.Test.Projection;

public class FlatClassesProjectionTest
{
    [Theory]
    [MemberData(nameof(FlatClassesTestDataGenerator.FlatClassesCollection),
        MemberType = typeof(FlatClassesTestDataGenerator))]
    public void ProjectTo_Should_Project_Flat_Classes_Correctly(IEnumerable<Customer> sources,
        IEnumerable<CustomerDto> destinations)
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });

        List<CustomerDto> result = mapper
            .ProjectTo<Customer, CustomerDto>(sources.AsQueryable())
            .ToList();

        List<CustomerDto> expected = destinations.ToList();

        Assert.Equal(expected.Count, result.Count);

        var projected = result.Zip(
            expected,
            (actual, expectedItem) => (Actual: actual, Expected: expectedItem)
        );

        Assert.All(projected, item =>
        {
            Assert.Equal(item.Expected.Id, item.Actual.Id);
            Assert.Equal(item.Expected.GivenName, item.Actual.GivenName);
            Assert.Equal(item.Expected.Surname, item.Actual.Surname);
            Assert.Equal(item.Expected.Email, item.Actual.Email);
            Assert.Equal(item.Expected.DateOfBirth, item.Actual.DateOfBirth);
            Assert.Equal(item.Expected.IsActive, item.Actual.IsActive);
        });
    }

    [Fact]
    public void ProjectTo_Should_Return_Empty_Query_When_Source_Is_Empty()
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });

        IQueryable<CustomerDto> result = mapper.ProjectTo<Customer, CustomerDto>(Array.Empty<Customer>().AsQueryable());

        Assert.Empty(result);
    }

    [Fact]
    public void ProjectTo_With_Null_Source_Should_Throw()
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>();

        IQueryable<Customer> source = null!;

        Assert.Throws<ArgumentNullException>(() => mapper.ProjectTo<Customer, CustomerDto>(source));
    }

    [Fact]
    public void ProjectTo_Should_Project_By_Convention_Without_Profile()
    {
        Customer[] sources =
        [
            new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            }
        ];

        Mapper mapper = new();

        CustomerDto result = mapper
            .ProjectTo<Customer, CustomerDto>(sources.AsQueryable())
            .Single();

        Assert.Equal(sources[0].Id, result.Id);
        Assert.Equal(sources[0].DateOfBirth, result.DateOfBirth);
        Assert.Equal(sources[0].IsActive, result.IsActive);
        Assert.Equal(string.Empty, result.GivenName);
        Assert.Equal(string.Empty, result.Surname);
        Assert.Equal(string.Empty, result.Email);
    }

    [Fact]
    public void ProjectTo_Should_Project_Custom_Expression_Bindings()
    {
        Customer[] sources =
        [
            new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            }
        ];

        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName + " " + s.LastName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });

        CustomerDto result = mapper
            .ProjectTo<Customer, CustomerDto>(sources.AsQueryable())
            .Single();

        Assert.Equal(sources[0].Id, result.Id);
        Assert.Equal("John Doe", result.GivenName);
        Assert.Equal(sources[0].LastName, result.Surname);
        Assert.Equal(sources[0].EmailAddress, result.Email);
        Assert.Equal(sources[0].DateOfBirth, result.DateOfBirth);
        Assert.Equal(sources[0].IsActive, result.IsActive);
    }

    [Fact]
    public void ProjectTo_Should_Handle_Null_Source_Members()
    {
        Customer[] sources =
        [
            new Customer
            {
                Id = 1,
                FirstName = null!,
                LastName = null!,
                EmailAddress = null!,
                DateOfBirth = DateTime.MinValue,
                IsActive = false
            }
        ];

        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });

        CustomerDto result = mapper
            .ProjectTo<Customer, CustomerDto>(sources.AsQueryable())
            .Single();

        Assert.Equal(sources[0].Id, result.Id);
        Assert.Null(result.GivenName);
        Assert.Null(result.Surname);
        Assert.Null(result.Email);
        Assert.Equal(sources[0].DateOfBirth, result.DateOfBirth);
        Assert.Equal(sources[0].IsActive, result.IsActive);
    }

    [Fact]
    public void ProjectTo_Should_Be_Deferred()
    {
        List<Customer> sources = [];

        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });

        IQueryable<CustomerDto> query = mapper.ProjectTo<Customer, CustomerDto>(sources.AsQueryable());

        sources.Add(new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            DateOfBirth = new DateTime(1990, 1, 15),
            IsActive = true
        });

        CustomerDto result = query.Single();

        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.GivenName);
        Assert.Equal("Doe", result.Surname);
        Assert.Equal("john.doe@example.com", result.Email);
    }

    [Fact]
    public void ProjectTo_Should_Build_Ef_Translatable_Queryable_Expression()
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });

        IQueryable<CustomerDto> query = mapper.ProjectTo<Customer, CustomerDto>(
            new List<Customer>().AsQueryable()
        );
        
        MethodCallExpression selectCall = Assert.IsType<MethodCallExpression>(query.Expression, exactMatch: false);

        Assert.Equal(nameof(Queryable.Select), selectCall.Method.Name);
        Assert.Equal(typeof(Queryable), selectCall.Method.DeclaringType);

        UnaryExpression quotedSelector = Assert.IsType<UnaryExpression>(selectCall.Arguments[1]);
        Assert.Equal(ExpressionType.Quote, quotedSelector.NodeType);

        Expression<Func<Customer, CustomerDto>> selector =
            Assert.IsType<Expression<Func<Customer, CustomerDto>>>(quotedSelector.Operand, exactMatch: false);

        MemberInitExpression body = Assert.IsType<MemberInitExpression>(selector.Body);

        Assert.Equal(typeof(CustomerDto), body.Type);

        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(CustomerDto.Id));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(CustomerDto.GivenName));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(CustomerDto.Surname));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(CustomerDto.Email));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(CustomerDto.DateOfBirth));
        Assert.Contains(body.Bindings, binding => binding.Member.Name == nameof(CustomerDto.IsActive));
    }

    [Fact]
    public void ProjectTo_Should_Be_Executable_By_Ef_Core()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();

        DbContextOptions<TestProjectionDbContext> options = new DbContextOptionsBuilder<TestProjectionDbContext>()
            .UseSqlite(connection)
            .Options;

        using (TestProjectionDbContext dbContext = new(options))
        {
            dbContext.Database.EnsureCreated();

            dbContext.Customers.Add(new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            });

            dbContext.SaveChanges();
        }

        using (TestProjectionDbContext dbContext = new(options))
        {
            Mapper mapper = new();
            mapper.CreateMap<Customer, CustomerDto>(profile =>
            {
                profile
                    .Bind(s => s.FirstName, d => d.GivenName)
                    .Bind(s => s.LastName, d => d.Surname)
                    .Bind(s => s.EmailAddress, d => d.Email);
            });

            CustomerDto result = mapper
                .ProjectTo<Customer, CustomerDto>(dbContext.Customers)
                .Single();

            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.GivenName);
            Assert.Equal("Doe", result.Surname);
            Assert.Equal("john.doe@example.com", result.Email);
            Assert.Equal(new DateTime(1990, 1, 15), result.DateOfBirth);
            Assert.True(result.IsActive);
        }
    }
}