using System.Linq.Expressions;
using System.Reflection;

namespace LazyMapper.Binding;

internal interface IResolverBinding
{
    PropertyInfo DestinationProperty { get; }
    
    LambdaExpression ResolverExpression { get; }
    
    object? Resolve(object source);
}

internal interface IResolverBinding<TSource> : IResolverBinding
{
    object? Resolve(TSource source);
}