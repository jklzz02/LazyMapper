namespace LazyMapper.Lib.Profile;

public interface IMapProfile
{
    public ResolverBase? Resolver(ResolverKey key);
}