using System.Linq.Expressions;
using System.Reflection;
using LazyMapper.Lib.Binding;
using LazyMapper.Lib.Exceptions;

namespace LazyMapper.Lib.Profile;

/// <summary>
/// Represents a map profile for mapping objects of type <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TSource">The source type. Must be a class with a parameterless constructor.</typeparam>
/// <typeparam name="TDestination">The destination type. Must be a class with a parameterless constructor.</typeparam>
public class MapProfile<TSource, TDestination> : IMapProfile
    where TSource : class, new()
    where TDestination : class, new()
{
    private readonly Dictionary<BindingKey, MapBinding> _sourceBindings = new();
    private readonly Dictionary<BindingKey, MapBinding> _destinationBindings = new();
    private readonly HashSet<PropertyInfo> _ignored = [];
    
    private static readonly Type SourceType = typeof(TSource);
    private static readonly Type DestinationType = typeof(TDestination);
    
    private Action<TSource>? _beforeMap;
    private Action<TSource, TDestination>? _afterMap;

    protected internal MapProfile()
    {
    }

    /// <summary>
    /// Gets the key for this map profile.
    /// </summary>
    public ProfileKey Key
        => new ProfileKey
        {
            SourceType = SourceType,
            DestinationType = DestinationType
        };
    
    bool IMapProfile.IsIgnored(PropertyInfo propertyInfo)
        => _ignored.Contains(propertyInfo);

    MapBinding? IMapProfile.Binding(BindingKey binderKey)
        => _sourceBindings.GetValueOrDefault(binderKey) ?? _destinationBindings.GetValueOrDefault(binderKey);

    /// <summary>
    /// Binds two members of the source and destination types.
    /// </summary>
    /// <param name="sourceMemberSelector">The selector for the source member.</param>
    /// <param name="destinationMemberSelector">The selector for the destination member.</param>
    /// <typeparam name="TMember">The type of the member to bind</typeparam>
    /// <returns>
    /// The same instance of <see cref="MapProfile{TSource, TDestination}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="sourceMemberSelector"/> or <paramref name="destinationMemberSelector"/> is null.
    /// </exception>
    public MapProfile<TSource, TDestination> Bind<TMember>(
        Expression<Func<TSource, TMember>> sourceMemberSelector,
        Expression<Func<TDestination, TMember>> destinationMemberSelector)
    {
        ArgumentNullException.ThrowIfNull(sourceMemberSelector);
        ArgumentNullException.ThrowIfNull(destinationMemberSelector);
        
        MapBinding binding = new MapBinding
        {
            SourceProperty = ExtractProperty(sourceMemberSelector.Body),
            DestinationProperty = ExtractProperty(destinationMemberSelector.Body)
        };
        
        AddBinding(binding);
        return this;
    }
    
    /// <summary>
    /// Binds two members of the source and destination types.
    /// </summary>
    /// <param name="sourceMemberSelector">The selector for the source member.</param>
    /// <param name="destinationMemberSelector">The selector for the destination member.</param>
    /// <returns>
    /// The same instance of <see cref="MapProfile{TSource, TDestination}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
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
        
        AddBinding(binding);
        return this;
    }
    
    /// <summary>
    /// Specify to ignore the selected member.
    /// </summary>
    /// <param name="memberSelector">The selector for the member to ignore</param>
    /// <typeparam name="TMember">The type of the member to ignore.</typeparam>
    /// <returns>The same instance of <see cref="MapProfile{TSource,TDestination}"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public MapProfile<TSource, TDestination> Ignore<TMember>(Expression<Func<TSource, TMember>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);
        
        PropertyInfo property = ExtractProperty(memberSelector.Body);

        _ignored.Add(property);
        _sourceBindings.Remove(new BindingKey
        {
            MemberName = property.Name,
            MemberType = property.PropertyType
        });

        return this;
    }
    
    /// <summary>
    /// Creates a new map profile that maps the destination members to the source members.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="MapProfile{TDestination, TSource}"/> that maps the destination members to the source members.
    /// </returns>
    public MapProfile<TDestination, TSource> Reverse()
    {
        MapProfile<TDestination, TSource> profile = new MapProfile<TDestination, TSource>();
        foreach (var resolver in _sourceBindings.Values)
        {
            profile.AddBinding(new MapBinding
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

    private void AddBinding(MapBinding binding)
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

        if (!_sourceBindings.TryAdd(sourceBinderKey, binding))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{sourceBinderKey.MemberName}' already exists."
            );
        }

        if (!_destinationBindings.TryAdd(destinationBinderKey, binding))
        {
            throw new MappingConfigurationException(
                $"A mapping for member '{destinationBinderKey.MemberName}' already exists."
            );
        }   
    }
}
