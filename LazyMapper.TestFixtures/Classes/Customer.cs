namespace LazyMapper.TestFixtures.Classes;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
}