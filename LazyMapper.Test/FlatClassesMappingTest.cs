using LazyMapper.Lib;
using LazyMapper.Test.Generators;
using LazyMapper.TestFixtures.Models;
using LazyMapper.TestFixtures.Dto;

namespace LazyMapper.Test;

public class FlatClassesMappingTest
{
    [Theory]
    [MemberData(nameof(FlatClassesTestDataGenerator.ForwardMapping), MemberType = typeof(FlatClassesTestDataGenerator))]
    public void Flat_Classes_Should_Map_Correctly(Customer source, CustomerDto destination)
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });
        
        CustomerDto result = mapper.Map<Customer, CustomerDto>(source);
        
        Assert.Equal(destination.Id, result.Id);
        Assert.Equal(destination.GivenName, result.GivenName);
        Assert.Equal(destination.Surname, result.Surname);
        Assert.Equal(destination.Email, result.Email);
        Assert.Equal(destination.DateOfBirth, result.DateOfBirth);
        Assert.Equal(destination.IsActive, result.IsActive);
    }
    
    [Theory]
    [MemberData(nameof(FlatClassesTestDataGenerator.ReverseMapping), MemberType = typeof(FlatClassesTestDataGenerator))]
    public void Flat_Classes_Should_Reverse_Map_Correctly(CustomerDto source, Customer destination)
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        })
        .ReverseMap();
        
        Customer result = mapper.Map<CustomerDto, Customer>(source);
        
        Assert.Equal(destination.Id, result.Id);
        Assert.Equal(destination.FirstName, result.FirstName);
        Assert.Equal(destination.LastName, result.LastName);
        Assert.Equal(destination.EmailAddress, result.EmailAddress);
        Assert.Equal(destination.DateOfBirth, result.DateOfBirth);
        Assert.Equal(destination.IsActive, result.IsActive);
    }
    
    [Theory]
    [MemberData(nameof(FlatClassesTestDataGenerator.RoundTrip), MemberType = typeof(FlatClassesTestDataGenerator))]
    public void Roundtrip_Should_Return_Same_Object(Customer source)
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        })
        .ReverseMap();
        
        CustomerDto result = mapper.Map<Customer, CustomerDto>(source);
        Customer roundTripped = mapper.Map<CustomerDto, Customer>(result);
        
        Assert.Equal(source.Id, roundTripped.Id);
        Assert.Equal(source.FirstName, roundTripped.FirstName);
        Assert.Equal(source.LastName, roundTripped.LastName);
        Assert.Equal(source.EmailAddress, roundTripped.EmailAddress);
        Assert.Equal(source.DateOfBirth, roundTripped.DateOfBirth);
        Assert.Equal(source.IsActive, roundTripped.IsActive);
    }
    
    [Theory]
    [MemberData(nameof(FlatClassesTestDataGenerator.FlatClassesCollection), MemberType = typeof(FlatClassesTestDataGenerator))]
    public void Collection_Should_Map_Correctly(IEnumerable<Customer> sources, IEnumerable<CustomerDto> destinations)
    {
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Bind(s => s.LastName, d => d.Surname)
                .Bind(s => s.EmailAddress, d => d.Email);
        });
        
        IEnumerable<CustomerDto> mapped = mapper.Map<Customer, CustomerDto>(sources);
        var result = mapped.Zip(
            destinations,
            (m, d) => (Actual: m, Expected: d)
        );
        
        Assert.All(result, r =>
        {
            Assert.Equal(r.Expected.Id, r.Actual.Id);
            Assert.Equal(r.Expected.GivenName, r.Actual.GivenName);
            Assert.Equal(r.Expected.Surname, r.Actual.Surname);
            Assert.Equal(r.Expected.Email, r.Actual.Email);
            Assert.Equal(r.Expected.DateOfBirth, r.Actual.DateOfBirth);
            Assert.Equal(r.Expected.IsActive, r.Actual.IsActive);
        });
    }

    [Fact]
    public void Null_Value_Should_Not_Be_Mapped()
    {
        Customer source = null!;
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>();
        
        Assert.Throws<ArgumentNullException>(() => mapper.Map<Customer, CustomerDto>(source));
    }

    [Fact]
    public void Map_With_Null_Profile_Should_Throw()
    {
        Customer source = new();
        Mapper mapper = new();
        
        Assert.Throws<InvalidOperationException>(() => mapper.Map<Customer, CustomerDto>(source));
    }
    
    [Fact]
    public void Ignored_Property_Should_Not_Be_Mapped()
    {
        Customer source = new Customer
        {
            Id = 1,
            FirstName = "John",
        };
        
        Mapper mapper = new();
        mapper.CreateMap<Customer, CustomerDto>(profile =>
        {
            profile
                .Bind(s => s.FirstName, d => d.GivenName)
                .Ignore(s => s.Id);
        });
        
        CustomerDto result = mapper.Map<Customer, CustomerDto>(source);
        
        Assert.Equal(source.FirstName, result.GivenName);
        Assert.NotEqual(source.Id, result.Id);
    }
}