using LazyMapper.Lib.Profile;

namespace LazyMapper.Lib.Configuration;

/// <summary>
/// Represents a configuration for mapping properties between instances of type <typeparamref name="TSource"/>
/// and type <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TSource">
/// The source type involved in the mapping. Must be a class with a parameterless constructor.
/// </typeparam>
/// <typeparam name="TDestination">
/// The destination type involved in the mapping. Must be a class with a parameterless constructor.
/// </typeparam>
public class MapConfiguration<TSource, TDestination>
    where TSource : class, new()
    where TDestination : class, new()
{    
    private readonly Mapper _mapper;
    private readonly MapProfile<TSource, TDestination> _profile;

    internal MapConfiguration(Mapper mapper, MapProfile<TSource, TDestination> profile)
    {
        _mapper = mapper;
        _profile = profile;
    }

    /// <summary>
    /// Registers an action to be executed after the mapping process between a source and destination object completes.
    /// </summary>
    /// <param name="action">The action to execute after the mapping process.</param>
    /// <returns>
    /// The same instance of <see cref="MapConfiguration{TSource, TDestination}"/>.
    /// </returns>
    public MapConfiguration<TSource, TDestination> AfterMap(Action<TSource, TDestination> action)
    {
        _profile.RegisterAfterMapHook(action);
        return this;
    }

    /// <summary>
    /// Registers an action to be executed before the mapping process between a source and destination object completes.
    /// </summary>
    /// <param name="action">The action to execute before the mapping process.</param>
    /// <returns>
    /// The same instance of <see cref="MapConfiguration{TSource, TDestination}"/>.
    /// </returns>
    public MapConfiguration<TSource, TDestination> BeforeMap(Action<TSource> action)
    {
        _profile.RegisterBeforeMapHook(action);
        return this;
    }

    /// <summary>
    /// Registers a reverse mapping profile.
    /// </summary>
    public void ReverseMap()
    {
        var reversed = _profile.Reverse();
        _mapper.Register(reversed);
    }
}