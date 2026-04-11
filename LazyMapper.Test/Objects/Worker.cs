namespace LazyMapper.Test.Objects;

internal class Worker : Person
{
    public string Company { get; set; } = "Lazy Company";
    public string Position { get; set; } = "Lazy Developer";
}