using System.Linq.Expressions;
using System.Reflection;

namespace LazyMapper.Binding;

internal class ResolverBinding<TSource, TMember> : IResolverBinding<TSource>
{
    private static readonly Dictionary<ResolverCacheKey, Func<TSource, TMember>> Resolvers = new();
    
    public required Expression<Func<TSource, TMember>> ResolverExpression { get; init; }
    
    public required PropertyInfo DestinationProperty { get; init; }
    
    LambdaExpression IResolverBinding.ResolverExpression
        => ResolverExpression;
    
    public object? Resolve(TSource source)
    {
        var key = new  ResolverCacheKey(typeof(TSource), DestinationProperty);

        if (Resolvers.TryGetValue(key, out var resolver))
        {
            return resolver(source);
        }
        
        resolver = ResolverExpression.Compile();
        Resolvers.Add(key, resolver);

        return resolver(source);
    }

    public object? Resolve(object source)
        => Resolve((TSource)source);
    
    private record ResolverCacheKey(Type SourceType, PropertyInfo DestinationProperty);
}
