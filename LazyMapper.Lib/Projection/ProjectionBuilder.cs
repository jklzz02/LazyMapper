using System.Linq.Expressions;
using System.Reflection;
using LazyMapper.Binding;
using LazyMapper.Collections;
using LazyMapper.Extensions;
using LazyMapper.Profile;

namespace LazyMapper.Projection;

internal class ProjectionBuilder
{
    private readonly Func<Type, Type, IMapProfile?> _getProfile;

    internal ProjectionBuilder(Func<Type, Type, IMapProfile?> getProfile)
    {
        _getProfile = getProfile;
    }

    internal Expression<Func<TSource, TDestination>> Build<TSource, TDestination>()
    {
        ParameterExpression srcParam = Expression.Parameter(typeof(TSource), "src");
        Expression body = BuildProjectionExpression(typeof(TSource), typeof(TDestination), srcParam);

        return Expression.Lambda<Func<TSource, TDestination>>(body, srcParam);
    }

    private Expression BuildProjectionExpression(
        Type sourceType,
        Type destType,
        Expression sourceExpr)
    {
        IMapProfile? profile = _getProfile(sourceType, destType);
        List<MapBinding> conventionBindings = ByConventionBinder.BuildBindings(sourceType, destType, profile).ToList();
        List<MemberBinding> memberBindings = new List<MemberBinding>();
        HashSet<PropertyInfo> mappedDestProperties = new HashSet<PropertyInfo>();

        foreach (MapBinding binding in conventionBindings)
        {
            MemberExpression sourceAccess = Expression.Property(sourceExpr, binding.SourceProperty);
            memberBindings.Add(BuildMemberBinding(binding.DestinationProperty, sourceAccess, binding.SourceProperty.PropertyType, binding.DestinationProperty.PropertyType));
            mappedDestProperties.Add(binding.DestinationProperty);
        }

        if (profile is not null)
        {
            IEnumerable<PropertyInfo> unmappedDestProps = destType.GetProperties()
                .Where(p => !mappedDestProperties.Contains(p));

            foreach (PropertyInfo destProp in unmappedDestProps)
            {
                BindingKey key = new BindingKey
                {
                    MemberName = destProp.Name,
                    MemberType = destProp.PropertyType
                };

                IResolverBinding? resolverBinding = profile.Resolver(key);
                if (resolverBinding is not null)
                {
                    Expression body = ExpressionParameterReplacer.Replace(
                        resolverBinding.ResolverExpression.Body,
                        resolverBinding.ResolverExpression.Parameters[0],
                        sourceExpr);

                    memberBindings.Add(Expression.Bind(resolverBinding.DestinationProperty, body));
                    continue;
                }

                MapBinding? customBinding = profile.Binding(key);
                if (customBinding is null)
                {
                    continue;
                }

                MemberExpression sourceAccess = Expression.Property(sourceExpr, customBinding.SourceProperty);
                memberBindings.Add(BuildMemberBinding(destProp, sourceAccess, customBinding.SourceProperty.PropertyType, destProp.PropertyType));
            }
        }

        return Expression.MemberInit(Expression.New(destType), memberBindings);
    }

    private MemberBinding BuildMemberBinding(
        PropertyInfo destProperty,
        MemberExpression sourceAccess,
        Type sourcePropType,
        Type destPropType)
    {
        if (sourcePropType.IsCollection())
        {
            Type sourceElementType = sourcePropType.CollectionElementType()!;
            Type destElementType = destPropType.CollectionElementType()!;

            Expression collectionExpr = CollectionHandler.BuildCollectionProjectionExpression(
                sourceAccess,
                sourceElementType,
                destElementType,
                destPropType,
                BuildElementProjection);

            return Expression.Bind(destProperty, collectionExpr);
        }

        IMapProfile? nestedProfile = _getProfile(sourcePropType, destPropType);
        if (nestedProfile is not null)
        {
            Expression nestedInit = BuildProjectionExpression(sourcePropType, destPropType, sourceAccess);
            return Expression.Bind(destProperty, nestedInit);
        }

        return Expression.Bind(destProperty, sourceAccess);
    }

    private Expression BuildElementProjection(Type sourceType, Type destType, Expression sourceExpr)
    {
        IMapProfile? elementProfile = _getProfile(sourceType, destType);

        if (elementProfile is not null)
        {
            return BuildProjectionExpression(sourceType, destType, sourceExpr);
        }

        return sourceExpr;
    }
}