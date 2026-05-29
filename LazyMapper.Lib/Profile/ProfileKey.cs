namespace LazyMapper.Lib.Profile;

/// <summary>
/// Represents a key for a map profile.
/// </summary>
public record ProfileKey
{
    /// <summary>
    /// Gets the source type of the profile.
    /// </summary>
    public required Type SourceType { get; init; }
    
    /// <summary>
    /// Gets the destination type of the profile.
    /// </summary>
    public required Type DestinationType { get; init; }
}