using System.Reflection;

namespace LazyMapper.Binding;

internal interface IResolverBinding
{
    PropertyInfo DestinationProperty { get; }
    
    object? Resolve(object source);
}

internal interface IResolverBinding<TSource> : IResolverBinding
{
    object? Resolve(TSource source);
}