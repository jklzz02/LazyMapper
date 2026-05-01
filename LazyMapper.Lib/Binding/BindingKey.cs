namespace LazyMapper.Lib.Binding;

/// <summary>
/// Represents a key for a binding.
/// </summary>
public record BindingKey
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