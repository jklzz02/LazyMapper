namespace LazyMapper.Test.Objects;

internal class Student : Person
{
    public string StudentName { get; set; } = "Lazy Student";
    public string School { get; set; } = "Lazy University";
    public string Class { get; set; } = "Lazy Class";
}