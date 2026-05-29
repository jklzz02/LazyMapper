using System.Reflection;

namespace LazyMapper.Lib.Binding;

public interface IResolverBinding
{
    PropertyInfo DestinationProperty { get; }
    
    object? Resolve(object source);
}

public interface IResolverBinding<TSource> : IResolverBinding
{
    object? Resolve(TSource source);
}