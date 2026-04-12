namespace LazyMapper.Test.Objects;

internal class A
{
    public string Name { get; set; } = "Lazy A";
    public B? Child { get; set; }
}

internal class B
{
    public string Title { get; set; } = "Lazy B";
    public A? Parent { get; set; }
}

internal class ADto
{
    public string Name { get; set; } = "Lazy ADto";
    public BDto? Child { get; set; }
}

internal class BDto
{
    public string Title { get; set; } = "Lazy BDto";
    public ADto? Parent { get; set; }
}