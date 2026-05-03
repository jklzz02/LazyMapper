namespace LazyMapper.Lib.Profile;

public record ProfileKey
{
    public required Type SourceType { get; init; }
    public required Type DestinationType { get; init; }
}