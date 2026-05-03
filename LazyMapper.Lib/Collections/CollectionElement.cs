namespace LazyMapper.Lib.Collections;

internal record CollectionElement
{
    public required object Value { get; init; }
    public required Type ItemType { get; init; }
}