using System.Reflection;

namespace LazyMapper.Lib.Binding;

/// <summary>
/// Represents a binding between a source property and a destination property.
/// </summary>
public class MapBinding
{
    /// <summary>
    /// Gets the destination property.
    /// </summary>
    public  required PropertyInfo DestinationProperty { get; init; }
    
    /// <summary>
    /// Gets the source property.
    /// </summary>
    public required PropertyInfo SourceProperty { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether the destination property is a nested resolution.
    /// </summary>
    public bool IsNestedResolution => 
        SourceProperty.PropertyType != DestinationProperty.PropertyType &&
        !DestinationProperty.PropertyType.IsValueType &&
        DestinationProperty.PropertyType != typeof(string);
}