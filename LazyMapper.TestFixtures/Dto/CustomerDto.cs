namespace LazyMapper.TestFixtures.Dto;

public class CustomerDto
{
    public int Id { get; set; }
    public string GivenName { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
}