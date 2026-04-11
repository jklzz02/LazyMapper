using System.Reflection;
using LazyMapper.Lib.Exceptions;
using LazyMapper.Lib.Profile;

namespace LazyMapper.Lib;

public class Mapper
{
    private readonly Dictionary<ProfileKey, IMapProfile> _profiles = new();

    public TDestination Map<TSource, TDestination>(TSource source)
        where TSource : class, new()
        where TDestination : class, new()
    {
        Type sourceType = typeof(TSource);
        Type destinationType = typeof(TDestination);
        
        IMapProfile? profile = GetProfile<TSource, TDestination>();

        if (profile == null)
        {
            throw new InvalidOperationException($"Cannot map '{sourceType.FullName}' to type '{destinationType.FullName}'");
        }

        return (TDestination)MapNested(source, sourceType, destinationType, profile);
    }
    
    private object MapNested(object source, Type sourceType, Type destType, IMapProfile profile)
    {
        PropertyInfo[] sourceProperties = ExtractMappableProperties(sourceType);
        PropertyInfo[] destProperties = ExtractMappableProperties(destType);
        
        Dictionary<PropertyInfo, PropertyInfo> propMap = CreatePropertyMap(
            sourceProperties, 
            destProperties);

        object destination = Activator.CreateInstance(destType)!;

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
        
        foreach (PropertyInfo sourceProperty in unmappedProperties)
        {
            ResolverKey key = new ResolverKey
            {
                SourceMemberName = sourceProperty.Name,
                SourceMemberType = sourceProperty.PropertyType
            };
            
            ResolverBase? resolver = profile.Resolver(key);
            
            if (sourceProperty.PropertyType == resolver?.DestinationProperty.PropertyType)
            {
                resolver.DestinationProperty.SetValue(destination, resolver.InvokeSelector(source));
            }
            else
            {
                
                var sourceValue = sourceProperty.GetValue(source);
                if (sourceValue == null)
                {
                    continue;
                }
                
                object mappedValue = MapNested(
                    sourceValue,
                    sourceProperty.PropertyType,
                    resolver?.DestinationProperty.PropertyType ?? throw new InvalidOperationException(),
                    profile
                );
                
                resolver?.DestinationProperty.SetValue(destination, mappedValue);
            }
        }

        return destination;
    }

    public void CreateMap<TSource, TDestination>(Action<MapProfile<TSource, TDestination>> mapConfiguration)
        where TSource : class, new()
        where TDestination : class, new()
    {
        MapProfile<TSource, TDestination> profile = new MapProfile<TSource, TDestination>();
        mapConfiguration(profile);
        
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
    }

    public Mapper Register<TProfile>() where TProfile : IMapProfile, new()
    {
        Type type = typeof(TProfile);
        if (!IsMapProfile(type))
        {
            throw new InvalidOperationException($"Cannot register type '{type.FullName}' as a profile");
        }
        
        if (!_profiles.TryAdd(GetProfileKey(type), new TProfile()))
        {
            throw new DuplicateProfilesException(type);
        }

        return this;
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

    private IMapProfile? GetProfile<TSource, TDestination>()
        where TSource : class, new()
        where TDestination : class, new()
    {
        ProfileKey key = new ProfileKey
        {
            SourceType = typeof(TSource),
            DestinationType = typeof(TDestination)
        };
        
        var profile = _profiles.GetValueOrDefault(key);
        
        if (profile is MapProfile<TSource, TDestination> mapProfile)
        {
            return mapProfile;
        }

        return null;
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
            (src) => new { src.Name, src.PropertyType },
            (dest) => new { dest.Name, dest.PropertyType },
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