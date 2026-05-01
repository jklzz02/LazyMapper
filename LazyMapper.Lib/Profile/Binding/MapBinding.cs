using System.Reflection;

namespace LazyMapper.Lib.Profile.Binding;

public class MapBinding
{

    public  required PropertyInfo DestinationProperty { get; init; }
    
    public required PropertyInfo SourceProperty { get; init; }
    
    public bool IsNestedResolution => 
        SourceProperty.PropertyType != DestinationProperty.PropertyType &&
        !DestinationProperty.PropertyType.IsValueType &&
        DestinationProperty.PropertyType != typeof(string);
}