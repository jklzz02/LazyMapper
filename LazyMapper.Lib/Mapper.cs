using System.Reflection;
using LazyMapper.Lib.Configuration;
using LazyMapper.Lib.Exceptions;
using LazyMapper.Lib.Extensions;
using LazyMapper.Lib.Binding;
using LazyMapper.Lib.Profile;
using LazyMapper.Lib.Profile.Keys;

namespace LazyMapper.Lib;

public class Mapper
{
    private readonly Dictionary<ProfileKey, IMapProfile> _profiles = new();
    private readonly Dictionary<object, object> _mapped = new(ReferenceEqualityComparer.Instance);

    public TDestination Map<TSource, TDestination>(TSource source)
        where TSource : class, new()
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        
        _mapped.Clear();
        Type sourceType = typeof(TSource);
        Type destinationType = typeof(TDestination);
        
        IMapProfile? profile = GetProfile<TSource, TDestination>();
        
        if (profile == null)
        {
            throw new InvalidOperationException(
                $"Cannot map '{sourceType.FullName}' to type '{destinationType.FullName}'"
            );
        }
        
        profile.InvokeBeforeMap(source);
        TDestination result = (TDestination)Map(source, sourceType, destinationType, profile);
        profile.InvokeAfterMap(source, result);
        
        return result;
    }

    public IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
        where TSource : class, new()
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select(Map<TSource, TDestination>);
    }
    
    private object Map(object source, Type sourceType, Type destType, IMapProfile profile)
    {
        if (_mapped.TryGetValue(source, out var mapped))
        {
            return mapped;
        }
        
        IEnumerable<MapBinding> bindings =  ByConventionBinder
            .BuildBindings(sourceType, destType, profile)
            .ToList();

        object destination = Activator.CreateInstance(destType)!;
        _mapped[source] = destination;

        foreach (MapBinding resolver in bindings)
        {
            resolver.DestinationProperty.SetValue(destination, resolver.SourceProperty.GetValue(source));
        }
        
        PropertyInfo[] unmappedProperties = destType.GetProperties()
            .Except(bindings.Select(r => r.DestinationProperty))
            .ToArray();

        if (unmappedProperties.Length == 0)
        {
            return destination;
        }

        foreach (PropertyInfo destinationProperty in unmappedProperties)
        {
            BindingKey key = new BindingKey
            {
                MemberName = destinationProperty.Name,
                MemberType = destinationProperty.PropertyType
            };
        
            MapBinding? resolver = profile.Binding(key);

            var sourceValue = resolver?.SourceProperty.GetValue(source);
        
            if (resolver is null || sourceValue is null)
            {
                continue;
            }
            
            if (resolver.SourceProperty.PropertyType.IsCollection())
            {
                var result = MapCollection(
                    sourceValue,
                    resolver.SourceProperty.PropertyType,
                    resolver.DestinationProperty.PropertyType);

                if (result is null)
                {
                    continue;
                }
                resolver.DestinationProperty.SetValue(destination, result);
            }

            if (!resolver.IsNestedResolution)
            {
                destinationProperty.SetValue(destination, sourceValue);
                continue;
            }

            IMapProfile? nestedProfile = GetProfile(
                resolver.SourceProperty.PropertyType,
                resolver.DestinationProperty.PropertyType);

            if (nestedProfile is null)
            {
                continue;
            }
            
            var mappedValue = Map(
                sourceValue,
                resolver.SourceProperty.PropertyType,
                resolver.DestinationProperty.PropertyType,
                nestedProfile
            );
            
            resolver.DestinationProperty.SetValue(destination, mappedValue);
        }
    
        return destination;
    }
    
    private object? MapCollection(
        object sourceValue,
        Type sourceCollectionType,
        Type destCollectionType)
    {
        Type? sourceElementType = sourceCollectionType.CollectionElementType();
        Type? destElementType   = destCollectionType.CollectionElementType();

        if (sourceElementType == null || destElementType == null)
            return null;

        var items = CollectionHandler.ExtractCollectionItems(sourceValue, sourceCollectionType);

        List<object> mappedItems;

        if (sourceElementType.IsCollection())
        {
            mappedItems = items
                .Select(item => MapCollection(item.Value, sourceElementType, destElementType))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();
        }
        else
        {
            IMapProfile? elementProfile = GetProfile(sourceElementType, destElementType);
            if (elementProfile is null)
                return null;

            mappedItems = items
                .Select(item => Map(item.Value, sourceElementType, destElementType, elementProfile))
                .ToList();
        }

        return CollectionHandler.ReconstructCollection(mappedItems, destElementType, destCollectionType);
    }
    
    public MapConfiguration<TSource, TDestination> CreateMap<TSource, TDestination>()
        where TSource : class, new()
        where TDestination : class, new()
        => CreateMap<TSource, TDestination>(null);

    public MapConfiguration<TSource, TDestination> CreateMap<TSource, TDestination>(
        Action<MapProfile<TSource, TDestination>>? mapConfigurations)
        where TSource : class, new()
        where TDestination : class, new()
    {
        MapProfile<TSource, TDestination> profile = new MapProfile<TSource, TDestination>();
        mapConfigurations?.Invoke(profile);
        
        Type sourceType = typeof(TSource);
        Type destinationType = typeof(TDestination);

        ProfileKey profileKey = new ProfileKey
        {
            SourceType = sourceType,
            DestinationType = destinationType
        };

        if (!_profiles.TryAdd(profileKey, profile))
        {
            throw new DuplicateProfilesException(sourceType, destinationType);
        }
        
        return new MapConfiguration<TSource, TDestination>(this, profile);
    }

    public void Register<TProfile>() where TProfile : new()
    {
        Type type = typeof(TProfile);
        
        if (!IsMapProfile(type))
        {
            throw new InvalidOperationException($"Cannot register type '{type.FullName}' as a profile");
        }

        IMapProfile profile = (IMapProfile)new TProfile();
        
        if (!_profiles.TryAdd(profile.Key, profile))
        {
            throw new DuplicateProfilesException(type);
        }
    }

    public void Register<TSource, TDestination>(MapProfile<TSource, TDestination> profile)
        where TSource : class, new()
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(profile);
        var core = (IMapProfile)profile;

        if (!_profiles.TryAdd(core.Key, core))
        {
            throw new DuplicateProfilesException(profile.GetType());
        }
    }
    
    public void RegisterProfilesFromAssembly(Assembly assembly)
    {
        List<Type> mapProfiles = assembly
            .GetTypes()
            .Where(IsMapProfile)
            .ToList();

        foreach (var profile in mapProfiles)
        {
            bool registered = _profiles.TryAdd(GetProfileKey(profile), InstantiateProfile(profile));
            if (!registered)
            {
                throw new DuplicateProfilesException(profile);
            }
        }
    }
    
    private IMapProfile? GetProfile(ProfileKey key) 
        => _profiles.GetValueOrDefault(key);
    
    private IMapProfile? GetProfile(Type sourceType, Type destType)
        => _profiles.GetValueOrDefault(new ProfileKey
        {
            SourceType = sourceType,
            DestinationType = destType
        });

    private IMapProfile? GetProfile<TSource, TDestination>()
        where TSource : class, new()
        where TDestination : class, new()
    {
        ProfileKey key = new ProfileKey
        {
            SourceType = typeof(TSource),
            DestinationType = typeof(TDestination)
        };
        
        return _profiles.GetValueOrDefault(key);
    }

    private IMapProfile InstantiateProfile(Type profileType)
    {
        if (profileType.GetConstructor(Type.EmptyTypes) == null)
        {
            throw new InvalidOperationException(
                $"Cannot instantiate profile '{profileType.FullName}' without parameterless constructor."
            );
        }
        
        return (IMapProfile) Activator.CreateInstance(profileType)!;
    }

    private ProfileKey GetProfileKey(Type profileType)
    {
        Type[] genericArguments = profileType.BaseType!.GetGenericArguments();
        return new ProfileKey
        {
            SourceType = genericArguments[0],
            DestinationType = genericArguments[1]
        };
    }
    
    private static bool IsMapProfile(Type type)
        => type.BaseType is { IsGenericType: true } baseType 
           && baseType.GetGenericTypeDefinition() == typeof(MapProfile<,>);
}