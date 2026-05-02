
using LazyMapper.TestFixtures.Classes;
using LazyMapper.TestFixtures.Dto;

namespace LazyMapper.Test.Generators;

public class FlatClassesTestDataGenerator
{
     public static TheoryData<Customer, CustomerDto> ForwardMapping()=> new()
    {
        {
            new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            },
            new CustomerDto
            {
                Id = 1,
                GivenName = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            }
        },
        
        {
            new Customer
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                EmailAddress = "jane.smith@example.com",
                DateOfBirth = new DateTime(1985, 6, 20),
                IsActive = false
            },
            new CustomerDto
            {
                Id = 2,
                GivenName = "Jane",
                Surname = "Smith",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateTime(1985, 6, 20),
                IsActive = false
            }
        },
        
        {
            new Customer
            {
                Id = 3,
                FirstName = null!,
                LastName = null!,
                EmailAddress = null!,
                DateOfBirth = DateTime.MinValue,
                IsActive = false
            },
            new CustomerDto
            {
                Id = 3,
                GivenName = null!,
                Surname = null!,
                Email = null!,
                DateOfBirth = DateTime.MinValue,
                IsActive = false
            }
        },
        
        {
            new Customer
            {
                Id = 4,
                FirstName = "",
                LastName = "",
                EmailAddress = "",
                DateOfBirth = DateTime.MaxValue,
                IsActive = true
            },
            new CustomerDto
            {
                Id = 4,
                GivenName = "",
                Surname = "",
                Email = "",
                DateOfBirth = DateTime.MaxValue,
                IsActive = true
            }
        }
    };

    public static TheoryData<CustomerDto, Customer> ReverseMapping() => new()
    {
        {
            new CustomerDto
            {
                Id = 1,
                GivenName = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            },
            new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true
            }
        },
        {
            new CustomerDto
            {
                Id = 2,
                GivenName = null!,
                Surname = null!,
                Email = null!,
                DateOfBirth = DateTime.MinValue,
                IsActive = false
            },
            new Customer
            {
                Id = 2,
                FirstName = null!,
                LastName = null!,
                EmailAddress = null!,
                DateOfBirth = DateTime.MinValue,
                IsActive = false
            }
        }
    };
    
    public static TheoryData<IEnumerable<Customer>, IEnumerable<CustomerDto>> FlatClassesCollection => new()
    {
        {
            [],
            []
        },
        {
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
            ],
            [
                new CustomerDto
                {
                    Id = 1,
                    GivenName = "John",
                    Surname = "Doe",
                    Email = "john.doe@example.com",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    IsActive = true
                }
            ]
        },
        {
            [
                new Customer
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    EmailAddress = "john.doe@example.com",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    IsActive = true
                },
                new Customer
                {
                    Id = 2,
                    FirstName = null!,
                    LastName = null!,
                    EmailAddress = null!,
                    DateOfBirth = DateTime.MinValue,
                    IsActive = false
                },
                new Customer
                {
                    Id = 3,
                    FirstName = "",
                    LastName = "",
                    EmailAddress = "",
                    DateOfBirth = DateTime.MaxValue,
                    IsActive = true
                }
            ],
            [
                new CustomerDto
                {
                    Id = 1,
                    GivenName = "John",
                    Surname = "Doe",
                    Email = "john.doe@example.com",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    IsActive = true
                },
                new CustomerDto
                {
                    Id = 2,
                    GivenName = null!,
                    Surname = null!,
                    Email = null!,
                    DateOfBirth = DateTime.MinValue,
                    IsActive = false
                },
                new CustomerDto
                {
                    Id = 3,
                    GivenName = "",
                    Surname = "",
                    Email = "",
                    DateOfBirth = DateTime.MaxValue,
                    IsActive = true
                }
            ]
        }
    };

    public static TheoryData<Customer> RoundTrip() =>
    [
        new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            DateOfBirth = new DateTime(1990, 1, 15),
            IsActive = true
        },

        new Customer
        {
            Id = 2,
            FirstName = null!,
            LastName = null!,
            EmailAddress = null!,
            DateOfBirth = DateTime.MinValue,
            IsActive = false
        },

        new Customer
        {
            Id = 3,
            FirstName = "",
            LastName = "",
            EmailAddress = "",
            DateOfBirth = DateTime.MaxValue,
            IsActive = true
        }
    ];
}