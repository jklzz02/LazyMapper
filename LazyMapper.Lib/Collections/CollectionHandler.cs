using System.Collections;
using System.Linq.Expressions;
using LazyMapper.Extensions;

namespace LazyMapper.Collections;

internal static class CollectionHandler
{
    internal static IList<CollectionElement> ExtractCollectionItems(object collection, Type collectionType)
    {
        if (!collectionType.IsCollection())
        {
            throw new InvalidOperationException("Cannot extract collection items from non-collection type");
        }
    
        IEnumerable iterable = (IEnumerable)collection;
        Type? elementType = collectionType.CollectionElementType();
        
        if (elementType == null)
        {
            throw new InvalidOperationException("Cannot determine element type of collection");
        }
    
        return iterable
            .Cast<object>()
            .Select(item => new CollectionElement
            {
                Value = item,
                ItemType = item?.GetType() ?? elementType,
            })
            .ToList();
    }

    internal static object ReconstructCollection(
        IList<object> mappedElements,
        Type elementType,
        Type collectionType)
    {
        if (collectionType.IsArray)
        {
            var array = Array.CreateInstance(elementType, mappedElements.Count);
            for (int i = 0; i < mappedElements.Count; i++)
            {
                array.SetValue(mappedElements[i], i);
            }
            return array;
        }

        if (collectionType.IsGenericType && 
            collectionType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in mappedElements)
            {
                list.Add(item);
            }
            return list;
        }

        if (collectionType.IsGenericType && 
            collectionType.GetGenericTypeDefinition() == typeof(HashSet<>))
        {
            var hashSetType = typeof(HashSet<>).MakeGenericType(elementType);
            var hashSet = Activator.CreateInstance(hashSetType)!;
            var addMethod = hashSetType.GetMethod("Add")!;
            foreach (var item in mappedElements)
            {
                addMethod.Invoke(hashSet, [item]);
            }
            return hashSet;
        }
        
        if (collectionType.IsGenericType &&
            collectionType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var genericArgs = collectionType.GetGenericArguments();
            var dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
            var dict = (IDictionary)Activator.CreateInstance(dictType)!;

            var keyProp = elementType.GetProperty("Key")!;
            var valueProp = elementType.GetProperty("Value")!;

            foreach (var item in mappedElements)
            {
                dict.Add(keyProp.GetValue(item)!, valueProp.GetValue(item));
            }

            return dict;
        }

        if (collectionType.IsInterface)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in mappedElements)
            {
                list.Add(item);
            }
            return list;
        }

        var instance = Activator.CreateInstance(collectionType);
        if (instance is IList baseList)
        {
            foreach (var item in mappedElements)
            {
                baseList.Add(item);
            }
            return baseList;
        }

        var defaultListType = typeof(List<>).MakeGenericType(elementType);
        var defaultList = (IList)Activator.CreateInstance(defaultListType)!;
        foreach (var item in mappedElements)
        {
            defaultList.Add(item);
        }
        return defaultList;
    }
    
    internal static Expression BuildCollectionProjectionExpression(
        Expression sourceAccess,
        Type sourceElementType,
        Type destElementType,
        Type destCollectionType,
        Func<Type, Type, Expression, Expression> buildNestedProjection)
    {
        if (destCollectionType.IsGenericType &&
            destCollectionType.GetGenericTypeDefinition() == typeof(HashSet<>))
        {
            throw new InvalidOperationException(
                $"Cannot project into 'HashSet<{destElementType.Name}>' — " +
                "ToHashSet() cannot be translated to SQL. Use List<T> or array on your DTO instead."
            );
        }

        if (destCollectionType.IsGenericType &&
            destCollectionType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            throw new InvalidOperationException(
                $"Cannot project into 'Dictionary<,>' — " +
                "Dictionary is not supported in projections. Use List<T> or array on your DTO instead."
            );
        }

        var itemParam = Expression.Parameter(sourceElementType, "item");
        var itemProjection = buildNestedProjection(sourceElementType, destElementType, itemParam);
        var itemLambda = Expression.Lambda(itemProjection, itemParam);

        var selectCall = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Select),
            [sourceElementType, destElementType],
            sourceAccess,
            itemLambda);

        if (destCollectionType.IsArray)
        {
            return Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.ToArray),
                [destElementType],
                selectCall);
        }

        return Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.ToList),
            [destElementType],
            selectCall);
    }
}
