using System.Linq.Expressions;
using System.Reflection;
using LazyMapper.Lib.Exceptions;
using LazyMapper.Lib.Profile.Keys;
using LazyMapper.Lib.Profile.Resolvers;

namespace LazyMapper.Lib.Profile;

public class MapProfile<TSource, TDestination> : IMapProfile
    where TSource : class, new()
    where TDestination : class, new()
{
    private readonly Dictionary<ResolverKey, ResolverBase> _sourceResolvers = new();
    private readonly Dictionary<ResolverKey, ResolverBase> _destinationResolvers = new();
    
    internal MapProfile()
    {
    }
    
    internal bool HasResolver(ResolverKey key)
        => _sourceResolvers.ContainsKey(key) ||  _destinationResolvers.ContainsKey(key);

    public ResolverBase? Resolver(ResolverKey binderKey)
        => _sourceResolvers.GetValueOrDefault(binderKey) ?? _destinationResolvers.GetValueOrDefault(binderKey);
    
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
        
        ResolverKey sourceBinderKey = new ResolverKey
        { 
            MemberName = sourceMemberExpression.Member.Name,
            MemberType = sourceMemberExpression.Type,
        };

        ResolverKey destinationBinderKey = new ResolverKey
        {
            MemberName = destinationMemberExpression.Member.Name,
            MemberType = destinationMemberExpression.Type,
        };

        MemberResolver<TSource, TDestination> resolver =
            new MemberResolver<TSource, TDestination>
            {
                SourceMemberSelector = sourceMemberSelector.Compile(),
                SourceMemberType =  sourceMemberExpression.Type,
                DestinationProperty = destinationProperty,
                DestinationMemberType =  destinationProperty.PropertyType
            };

        if (!_sourceResolvers.TryAdd(sourceBinderKey, resolver))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{sourceBinderKey.MemberName}' already exists."
            );
        }

        if (!_destinationResolvers.TryAdd(destinationBinderKey, resolver))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{destinationBinderKey.MemberName}' already exists."
            );
        }

        return this;
    }
}
