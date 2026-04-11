using LazyMapper.Lib.Profile.Keys;
using LazyMapper.Lib.Profile.Resolvers;

namespace LazyMapper.Lib.Profile;

public interface IMapProfile
{
    public ResolverBase? Resolver(ResolverKey key);
}