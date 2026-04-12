using LazyMapper.Lib.Profile.Keys;
using LazyMapper.Lib.Profile.Resolvers;

namespace LazyMapper.Lib.Profile;

public interface IMapProfile
{
    internal ProfileKey Key { get; }
    public MemberResolver? Resolver(ResolverKey key);
}