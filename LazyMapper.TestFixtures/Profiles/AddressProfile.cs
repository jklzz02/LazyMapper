using LazyMapper.Lib.Profile;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.TestFixtures.Profiles;

public class AddressProfile : MapProfile<Address, AddressDto>
{
    public AddressProfile()
    {
        Bind(address => address.PostalCode, dto => dto.ZipCode);
    }
}
