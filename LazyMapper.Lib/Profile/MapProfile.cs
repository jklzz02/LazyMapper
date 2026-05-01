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
    private readonly HashSet<PropertyInfo> _ignored = [];
    
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

    public bool IsIgnored(PropertyInfo propertyInfo)
        => _ignored.Contains(propertyInfo);

    public MemberResolver? Resolver(ResolverKey binderKey)
        => _sourceResolvers.GetValueOrDefault(binderKey) ?? _destinationResolvers.GetValueOrDefault(binderKey);

    public MapProfile<TSource, TDestination> Bind(
        Expression<Func<TSource, object?>> sourceMemberSelector,
        Expression<Func<TDestination, object?>> destinationMemberSelector)
    {
        ArgumentNullException.ThrowIfNull(sourceMemberSelector);
        ArgumentNullException.ThrowIfNull(destinationMemberSelector);

        MemberResolver resolver = new MemberResolver
        {
            SourceProperty =  ExtractProperty(sourceMemberSelector.Body),
            DestinationProperty = ExtractProperty(destinationMemberSelector.Body)
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

    public MapProfile<TSource, TDestination> Ignore(Expression<Func<TSource, object?>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);
        
        PropertyInfo property = ExtractProperty(memberSelector.Body);

        _ignored.Add(property);
        _sourceResolvers.Remove(new ResolverKey
        {
            MemberName = property.Name,
            MemberType = property.PropertyType
        });

        return this;
    }
    
    private static PropertyInfo ExtractProperty(Expression expression)
    {
        if (expression is not MemberExpression memberExpression)
            throw new MappingConfigurationException(
                $"'{nameof(expression)}' must be a member expression, got: '{expression.NodeType}'."
            );
        
        if (memberExpression.Member is not PropertyInfo property)
            throw new MappingConfigurationException(
                $"'{nameof(expression)}' must be a property expression, got: '{memberExpression.Member.MemberType}'."
            );
        
        return property;
    }

    private void AddResolver(MemberResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        
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
    }
}
