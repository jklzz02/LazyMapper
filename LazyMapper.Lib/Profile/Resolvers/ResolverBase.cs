using System.Reflection;

namespace LazyMapper.Lib.Profile.Resolvers;

public abstract class ResolverBase
{
    public  required PropertyInfo DestinationProperty { get; init; }
    
    public required Type SourceMemberType { get; init; }

    public bool IsNestedResolution
        => SourceMemberType != DestinationProperty.PropertyType;
    
    public abstract object? InvokeSelector(object source);
}