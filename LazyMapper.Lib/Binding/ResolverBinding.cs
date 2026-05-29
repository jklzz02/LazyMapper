using System.Reflection;

namespace LazyMapper.Binding;

internal class ResolverBinding<TSource, TMember> : IResolverBinding<TSource>
{
    public required Func<TSource, TMember> Resolver { get; init; }
    
    public required PropertyInfo DestinationProperty { get; init; }
    
    public object? Resolve(TSource source)
        => Resolver(source);

    public object? Resolve(object source)
        => Resolve((TSource)source);
}