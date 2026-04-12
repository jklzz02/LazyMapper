using System.Reflection;

namespace LazyMapper.Lib.Profile.Resolvers;

public class MemberResolver
{

    public  required PropertyInfo DestinationProperty { get; init; }
    
    public required PropertyInfo SourceProperty { get; init; }
    
    public bool IsNestedResolution
        => SourceProperty.PropertyType != DestinationProperty.PropertyType;
}