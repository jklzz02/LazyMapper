using System.Reflection;

namespace LazyMapper.Lib.Profile.Resolvers;

public abstract class ResolverBase
{
    public  required PropertyInfo DestinationProperty { get; init; }
    
    public required Type SourceMemberType { get; init; }
    
    public required Type DestinationMemberType { get; init; }

    public bool IsNestedResolution
        => SourceMemberType != DestinationMemberType;
    
    public abstract object? InvokeSelector(object source);
}