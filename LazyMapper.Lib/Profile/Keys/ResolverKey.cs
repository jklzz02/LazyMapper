namespace LazyMapper.Lib.Profile.Keys;

/// <summary>
/// Represents a key for a resolver.
/// </summary>
public record ResolverKey
{
    /// <summary>
    /// Gets the name of the source member.
    /// </summary>
    public required string MemberName { get; init; }
    
    /// <summary>
    /// Gets the type of the source member.
    /// </summary>
    public required Type MemberType { get; init; }
}