using System.Reflection;
using LazyMapper.Lib.Profile;

namespace LazyMapper.Lib.Binding;

internal static class ByConventionBinder
{
    internal static IEnumerable<MapBinding> BuildBindings(Type sourceType, Type destinationType, IMapProfile? profile = null)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(destinationType);
        
        PropertyInfo[] sourceProps = ExtractBindableProperties(sourceType, profile);
        PropertyInfo[] destinationProps = ExtractBindableProperties(destinationType, profile);

        IEnumerable<MapBinding> result = sourceProps
            .Join(destinationProps,
                src => new { src.Name, src.PropertyType },
                dest => new { dest.Name, dest.PropertyType },
                (src, dest) => new MapBinding
                {
                    SourceProperty = src,
                    DestinationProperty = dest
                }
            ).ToList();
        
        return result;
    }
    
    private static PropertyInfo[] ExtractBindableProperties(Type type, IMapProfile? profile = null)
    {
        PropertyInfo[] bindableProperties = type.GetProperties()
            .Where(p =>
            {
                if (profile != null && profile.IsIgnored(p))
                {
                    return false;
                }

                return p is { CanRead: true, CanWrite: true };
            })
            .ToArray();
        
        return bindableProperties;
    }
}
