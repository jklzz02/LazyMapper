namespace LazyMapper.Lib.Profile;

/// <summary>
/// Represents a key for a member.
/// </summary>
public record ResolverKey
{
    /// <summary>
    /// Gets the name of the source member.
    /// </summary>
    public required string SourceMemberName { get; init; }
    
    /// <summary>
    /// Gets the type of the source member.
    /// </summary>
    public required Type SourceMemberType { get; init; }
}