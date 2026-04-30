using System.Reflection;
using LazyMapper.Lib.Configuration;
using LazyMapper.Lib.Exceptions;
using LazyMapper.Lib.Extensions;
using LazyMapper.Lib.Profile;
using LazyMapper.Lib.Profile.Keys;
using LazyMapper.Lib.Profile.Resolvers;

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

        return (TDestination)Map(source, sourceType, destinationType, profile);
    }
    
    private object Map(object source, Type sourceType, Type destType, IMapProfile profile)
    {
        if (_mapped.TryGetValue(source, out var mapped))
        {
            return mapped;
        }
        
        PropertyInfo[] sourceProperties = ExtractMappableProperties(sourceType);
        PropertyInfo[] destProperties = ExtractMappableProperties(destType);
        
        Dictionary<PropertyInfo, PropertyInfo> propMap = CreatePropertyMap(
            sourceProperties, 
            destProperties);

        object destination = Activator.CreateInstance(destType)!;
        _mapped[source] = destination;

        foreach (var mappedProp in propMap)
        {
            mappedProp.Value.SetValue(destination, mappedProp.Key.GetValue(source));
        }
        
        PropertyInfo[] unmappedProperties = destProperties
            .Except(propMap.Values)
            .ToArray();

        if (unmappedProperties.Length == 0)
        {
            return destination;
        }

        foreach (PropertyInfo destinationProperty in unmappedProperties)
        {
            ResolverKey key = new ResolverKey
            {
                MemberName = destinationProperty.Name,
                MemberType = destinationProperty.PropertyType
            };
        
            MemberResolver? resolver = profile.Resolver(key);

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
    
    public IMapConfiguration CreateMap<TSource, TDestination>()
        where TSource : class, new()
        where TDestination : class, new()
        => CreateMap<TSource, TDestination>(null);

    public IMapConfiguration CreateMap<TSource, TDestination>(
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

    public void Register<TProfile>() where TProfile : IMapProfile, new()
    {
        Type type = typeof(TProfile);
        
        if (!IsMapProfile(type))
        {
            throw new InvalidOperationException($"Cannot register type '{type.FullName}' as a profile");
        }

        TProfile profile = new TProfile();
        
        if (!_profiles.TryAdd(profile.Key, profile))
        {
            throw new DuplicateProfilesException(type);
        }
    }

    public void Register(IMapProfile profile)
    {
        if (!_profiles.TryAdd(profile.Key, profile))
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

    private Dictionary<PropertyInfo, PropertyInfo> CreatePropertyMap(
        PropertyInfo[] sourceProps,
        PropertyInfo[] destinationProps)
    => sourceProps
        .Join(
            destinationProps,
            src => new { src.Name, src.PropertyType },
            dest => new { dest.Name, dest.PropertyType },
            (src, dest) => new {Source = src, Destination = dest}
        )
        .ToDictionary(
            pair => pair.Source,
            pair => pair.Destination);
    
    private PropertyInfo[] ExtractMappableProperties(Type type)
        => type.GetProperties()
            .Where(p => p is { CanRead: true, CanWrite: true })
            .ToArray();
    
    private bool IsMapProfile(Type type)
        => type.IsAssignableTo(typeof(IMapProfile));
}