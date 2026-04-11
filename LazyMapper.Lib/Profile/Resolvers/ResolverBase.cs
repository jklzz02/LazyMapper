using System.Reflection;

namespace LazyMapper.Lib.Profile;

public abstract class ResolverBase
{
    public  required PropertyInfo DestinationProperty { get; init; }
    
    public required Type MemberType { get; init; }
    
    public abstract object? InvokeSelector(object source);
}