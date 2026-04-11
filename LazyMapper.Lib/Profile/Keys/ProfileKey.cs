namespace LazyMapper.Lib.Profile.Keys;

public record ProfileKey
{
    public required Type SourceType { get; init; }
    public required Type DestinationType { get; init; }
}