using System.Collections;

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