using LazyMapper.Lib.Profile;

namespace LazyMapper.Lib.Configuration;

public interface IMapConfiguration<TSource, TDestination>
    where TSource : class, new()
    where TDestination : class, new()
{
    IMapConfiguration<TSource, TDestination> CreateMap(Action<MapProfile<TSource, TDestination>> mapConfigurations);
    void ReverseMap();
}