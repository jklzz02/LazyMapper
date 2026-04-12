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
    private readonly Dictionary<ResolverKey, MemberResolver> _sourceResolvers = new();
    private readonly Dictionary<ResolverKey, MemberResolver> _destinationResolvers = new();
    
    private static readonly Type SourceType = typeof(TSource);
    private static readonly Type DestinationType = typeof(TDestination);
    
    internal MapProfile()
    {
    }

    public ProfileKey Key
        => new ProfileKey
        {
            SourceType = SourceType,
            DestinationType = DestinationType
        };

    public MemberResolver? Resolver(ResolverKey binderKey)
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
        
        if (sourceMemberExpression.Member is not PropertyInfo sourceProperty)
        {
            throw new MappingConfigurationException(
                $"'{nameof(sourceMemberSelector)}' must be a property expression, got: '{destinationMemberExpression.Member.MemberType}'."
            );
        }
        
        if (destinationMemberExpression.Member is not PropertyInfo destinationProperty)
            throw new MappingConfigurationException(
                $"'{nameof(destinationMemberSelector)}' must be a property expression, got: '{destinationMemberExpression.Member.MemberType}'."
            );

        MemberResolver resolver = new MemberResolver
        {
            SourceProperty =  sourceProperty,
            DestinationProperty = destinationProperty,
        };
        
        AddResolver(resolver);
        return this;
    }
    
    public MapProfile<TDestination, TSource> Reverse()
    {
        MapProfile<TDestination, TSource> profile = new MapProfile<TDestination, TSource>();
        foreach (var resolver in _sourceResolvers.Values)
        {
            profile.AddResolver(new MemberResolver
            {
                SourceProperty = resolver.DestinationProperty,
                DestinationProperty = resolver.SourceProperty
            });
        }
        
        return profile; 
    }

    internal MapProfile<TSource, TDestination> AddResolver(MemberResolver resolver)
    {
        ResolverKey sourceBinderKey = new ResolverKey
        { 
            MemberName = resolver.SourceProperty.Name,
            MemberType = resolver.SourceProperty.PropertyType
        };

        ResolverKey destinationBinderKey = new ResolverKey
        {
            MemberName = resolver.DestinationProperty.Name,
            MemberType = resolver.DestinationProperty.PropertyType
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
