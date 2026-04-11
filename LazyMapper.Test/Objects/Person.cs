
namespace LazyMapper.Test.Objects;

internal class Person
{
    public string Name { get; set; } = "John";

    public string Surname { get; set; } = "Doe";

    public DateTime BirthDate { get; set; } = DateTime.UtcNow.AddYears(-20);

    public int Age
        => (DateTime.UtcNow.Date - BirthDate.Date).Days / 365;
}