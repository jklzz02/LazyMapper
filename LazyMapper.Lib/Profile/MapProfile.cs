using System.Linq.Expressions;
using System.Reflection;
using LazyMapper.Lib.Exceptions;
using LazyMapper.Lib.Profile.Keys;
using LazyMapper.Lib.Profile.Binding;

namespace LazyMapper.Lib.Profile;

public class MapProfile<TSource, TDestination> : IMapProfile
    where TSource : class, new()
    where TDestination : class, new()
{
    private readonly Dictionary<BindingKey, MapBinding> _sourceResolvers = new();
    private readonly Dictionary<BindingKey, MapBinding> _destinationResolvers = new();
    private readonly HashSet<PropertyInfo> _ignored = [];
    
    private static readonly Type SourceType = typeof(TSource);
    private static readonly Type DestinationType = typeof(TDestination);
    
    private Action<TSource>? _beforeMap;
    private Action<TSource, TDestination>? _afterMap;
    
    internal MapProfile()
    {
    }

    ProfileKey IMapProfile.Key
        => new ProfileKey
        {
            SourceType = SourceType,
            DestinationType = DestinationType
        };

    bool IMapProfile.IsIgnored(PropertyInfo propertyInfo)
        => _ignored.Contains(propertyInfo);

    MapBinding? IMapProfile.Binding(BindingKey binderKey)
        => _sourceResolvers.GetValueOrDefault(binderKey) ?? _destinationResolvers.GetValueOrDefault(binderKey);

    public MapProfile<TSource, TDestination> Bind(
        Expression<Func<TSource, object?>> sourceMemberSelector,
        Expression<Func<TDestination, object?>> destinationMemberSelector)
    {
        ArgumentNullException.ThrowIfNull(sourceMemberSelector);
        ArgumentNullException.ThrowIfNull(destinationMemberSelector);

        MapBinding binding = new MapBinding
        {
            SourceProperty =  ExtractProperty(sourceMemberSelector.Body),
            DestinationProperty = ExtractProperty(destinationMemberSelector.Body)
        };
        
        AddResolver(binding);
        return this;
    }
    
    public MapProfile<TSource, TDestination> Ignore(Expression<Func<TSource, object?>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);
        
        PropertyInfo property = ExtractProperty(memberSelector.Body);

        _ignored.Add(property);
        _sourceResolvers.Remove(new BindingKey
        {
            MemberName = property.Name,
            MemberType = property.PropertyType
        });

        return this;
    }
    
    internal MapProfile<TDestination, TSource> Reverse()
    {
        MapProfile<TDestination, TSource> profile = new MapProfile<TDestination, TSource>();
        foreach (var resolver in _sourceResolvers.Values)
        {
            profile.AddResolver(new MapBinding
            {
                SourceProperty = resolver.DestinationProperty,
                DestinationProperty = resolver.SourceProperty
            });
        }
        
        return profile;
    }
    
    void IMapProfile.InvokeBeforeMap(object source)
        => _beforeMap?.Invoke((TSource)source);

    void IMapProfile.InvokeAfterMap(object source, object destination)
        => _afterMap?.Invoke((TSource)source, (TDestination)destination);

    internal MapProfile<TSource, TDestination> RegisterBeforeMapHook(Action<TSource> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _beforeMap = action;
        return this;
    }

    internal MapProfile<TSource, TDestination> RegisterAfterMapHook(Action<TSource, TDestination> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _afterMap = action;
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

    private void AddResolver(MapBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        
        BindingKey sourceBinderKey = new BindingKey
        { 
            MemberName = binding.SourceProperty.Name,
            MemberType = binding.SourceProperty.PropertyType
        };

        BindingKey destinationBinderKey = new BindingKey
        {
            MemberName = binding.DestinationProperty.Name,
            MemberType = binding.DestinationProperty.PropertyType
        };

        if (!_sourceResolvers.TryAdd(sourceBinderKey, binding))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{sourceBinderKey.MemberName}' already exists."
            );
        }

        if (!_destinationResolvers.TryAdd(destinationBinderKey, binding))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{destinationBinderKey.MemberName}' already exists."
            );
        }   
    }
}
