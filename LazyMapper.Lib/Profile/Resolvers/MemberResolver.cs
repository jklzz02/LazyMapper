namespace LazyMapper.Lib.Profile.Resolvers;

/// <summary>
/// Holds the mapping information for a member.
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TDestination">The destination type</typeparam>
internal sealed class MemberResolver<TSource, TDestination> : ResolverBase
    where TSource : class, new()
    where TDestination : class, new()
{
    /// <summary>
    /// The member expression to access the source member.
    /// </summary>
    internal required Func<TSource, object> SourceMemberSelector { get; init; }

    public override object? InvokeSelector(object source)
    {
        return SourceMemberSelector((TSource) source);
    }
}