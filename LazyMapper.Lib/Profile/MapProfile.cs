using System.Linq.Expressions;
using System.Reflection;
using LazyMapper.Lib.Exceptions;

namespace LazyMapper.Lib.Profile;

public class MapProfile<TSource, TDestination> : IMapProfile
    where TSource : class, new()
    where TDestination : class, new()
{
    private readonly Dictionary<ResolverKey, ResolverBase> _resolvers = new();
    
    internal MapProfile()
    {
    }
    
    internal bool HasResolver(ResolverKey key)
        => _resolvers.ContainsKey(key);

    public ResolverBase? Resolver(ResolverKey binderKey)
        =>_resolvers.GetValueOrDefault(binderKey);

    public MapProfile<TSource, TDestination> Bind(
        Expression<Func<TSource, object>> sourceMemberSelector,
        Expression<Func<TDestination, object>> destinationMemberSelector)
    {
        ArgumentNullException.ThrowIfNull(sourceMemberSelector);
        ArgumentNullException.ThrowIfNull(destinationMemberSelector);
        
         if (sourceMemberSelector.Body is not MemberExpression sourceMemberExpression)
             throw new MappingConfigurationException(
                 $"'{nameof(sourceMemberSelector)}' must be a member expression, got: '{sourceMemberSelector}'."
             );

        if (destinationMemberSelector.Body is not MemberExpression destinationMemberExpression)
            throw new MappingConfigurationException(
                $"'{nameof(destinationMemberSelector)}' must be a member expression, got: '{destinationMemberSelector.Body}'."
            );
        
        if (destinationMemberExpression.Member is not PropertyInfo destinationProperty)
            throw new MappingConfigurationException(
                $"'{nameof(destinationMemberSelector)}' must be a property expression, got: '{destinationMemberExpression.Member.MemberType}'."
            );
        
        ResolverKey binderKey = new ResolverKey
        {
            SourceMemberName = sourceMemberExpression.Member.Name,
            SourceMemberType = sourceMemberExpression.Type,
        };

        MemberResolver<TSource, TDestination> resolver =
            new MemberResolver<TSource, TDestination>
            {
                SourceMemberSelector = sourceMemberSelector.Compile(),
                DestinationProperty = destinationProperty,
                MemberType = destinationProperty.PropertyType
            };

        if (!_resolvers.TryAdd(binderKey, resolver))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{binderKey.SourceMemberName}' already exists."
            );
        }

        return this;
    }
}
