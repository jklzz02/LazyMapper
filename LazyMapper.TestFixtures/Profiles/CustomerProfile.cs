using LazyMapper.Lib.Profile;
using LazyMapper.TestFixtures.Models;
using LazyMapper.TestFixtures.Dto;

namespace LazyMapper.TestFixtures.Profiles;

public class CustomerProfile : MapProfile<Customer, CustomerDto>
{
    public CustomerProfile()
    {
        Bind(c => c.FirstName, d => d.GivenName);
        Bind(c => c.LastName, d => d.Surname);
        Bind(c => c.EmailAddress, d => d.Email);
        Bind(c => c.DateOfBirth, d => d.DateOfBirth);
        Bind(c => c.IsActive, d => d.IsActive);
    }
}