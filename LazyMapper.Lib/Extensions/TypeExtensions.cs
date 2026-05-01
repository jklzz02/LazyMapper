using System.Collections;
using System.Reflection;
using LazyMapper.Lib.Profile;
using LazyMapper.Lib.Profile.Binding;

namespace LazyMapper.Lib.Extensions;

internal static class TypeExtensions
{
    private static readonly Type StringType = typeof(string);
    private static readonly Type EnumerableType = typeof(IEnumerable);
    private static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
    
    extension(Type type)
    {
        internal bool IsCollection()
            => type != StringType && EnumerableType.IsAssignableFrom(type);

        internal IEnumerable<MapBinding> BindProperties(Type otherType, IMapProfile? profile = null)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(otherType);
            
            PropertyInfo[] typeProp = type.ExtractMappableProperties(profile);
            PropertyInfo[] otherProp = otherType.ExtractMappableProperties(profile);

            return typeProp
                .Join(otherProp,
                    src => new { src.Name, src.PropertyType },
                    dest => new { dest.Name, dest.PropertyType },
                    (src, dest) => new MapBinding
                    {
                        SourceProperty = src,
                        DestinationProperty = dest
                    }
                );
        }

        internal PropertyInfo[] ExtractMappableProperties(IMapProfile? profile = null)
           => type.GetProperties()
                .Where(p =>
                {
                    if (profile != null && profile.IsIgnored(p))
                    {
                        return false;
                    }

                    return p is { CanRead: true, CanWrite: true };
                })
                .ToArray();

        internal Type? CollectionElementType()
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            
            HashSet<Type> elementTypes = [];

            if (type.IsGenericType && type.GetGenericTypeDefinition() == GenericEnumerableType)
            {
                elementTypes.Add(type.GetGenericArguments()[0]);
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == GenericEnumerableType)
                {
                    elementTypes.Add(interfaceType.GetGenericArguments()[0]);
                }
            }

            return elementTypes.Count == 1
                ? elementTypes.Single()
                : null;
        }
    }
}