using LazyMapper.Lib;
using LazyMapper.TestFixtures.Classes;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Profiles;

namespace LazyMapper.Test;

public class MapperConfigurationTest
{
    [Fact]
    public void Register_Should_Create_Mapper_Configuration()
    {
        Customer source = new();
        Mapper mapper = new Mapper();
        mapper.Register<CustomerProfile>();

        CustomerDto result = new();
        var ex  = Record.Exception(() =>
        {
            result = mapper.Map<Customer, CustomerDto>(source);
        });
        
        Assert.Null(ex);
        Assert.Equal(source.FirstName, result.GivenName);
        Assert.Equal(source.LastName, result.Surname);
        Assert.Equal(source.EmailAddress, result.Email);
        Assert.Equal(source.DateOfBirth, result.DateOfBirth);
        Assert.Equal(source.IsActive, result.IsActive);
    }

    [Fact]
    public void RegisterFromAssembly_Should_Create_Mapper_Configuration()
    {
        Customer source = new();
        Mapper mapper = new Mapper();
        mapper.RegisterProfilesFromAssembly(typeof(CustomerProfile).Assembly);
        
        CustomerDto result = new();
        var ex  = Record.Exception(() =>
        {
            result = mapper.Map<Customer, CustomerDto>(source);
        });
        
        Assert.Null(ex);
        Assert.Equal(source.FirstName, result.GivenName);
        Assert.Equal(source.LastName, result.Surname);
        Assert.Equal(source.EmailAddress, result.Email);
        Assert.Equal(source.DateOfBirth, result.DateOfBirth);
        Assert.Equal(source.IsActive, result.IsActive);
    }
    
    [Fact]
    public void Register_From_Instance_Should_Create_Mapper_Configuration()
    {
        Customer source = new();
        CustomerProfile profile = new();
        Mapper mapper = new Mapper();
        mapper.Register(profile);
        mapper.Register(profile.Reverse());

        CustomerDto result = new();
        var forwardEx  = Record.Exception(() =>
        {
            result = mapper.Map<Customer, CustomerDto>(source);
        });
        
        var reverseEx = Record.Exception(() => mapper.Map<CustomerDto, Customer>(result));
        
        Assert.Null(forwardEx);
        Assert.Null(reverseEx);
        Assert.Equal(source.FirstName, result.GivenName);
        Assert.Equal(source.LastName, result.Surname);
        Assert.Equal(source.EmailAddress, result.Email);
        Assert.Equal(source.DateOfBirth, result.DateOfBirth);
        Assert.Equal(source.IsActive, result.IsActive);
    }
}