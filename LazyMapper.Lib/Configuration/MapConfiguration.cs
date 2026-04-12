using LazyMapper.Lib.Profile;

namespace LazyMapper.Lib.Configuration;

public class MapConfiguration<TSource, TDestination> : IMapConfiguration<TSource, TDestination>
    where TSource : class, new()
    where TDestination : class, new()
{    
    private readonly Mapper _mapper;
    private readonly MapProfile<TSource, TDestination> _profile;

    internal MapConfiguration(Mapper mapper, MapProfile<TSource, TDestination> profile)
    {
        _mapper = mapper;
        _profile = profile;
    }
    
    public IMapConfiguration<TSource, TDestination> CreateMap(Action<MapProfile<TSource, TDestination>> mapConfigurations)
        => _mapper.CreateMap(mapConfigurations);

    public IMapConfiguration<TSource, TDestination> CreateMap()
        => _mapper.CreateMap<TSource, TDestination>();
    

    public void ReverseMap()
    {
        var reversed = _profile.Reverse();
        _mapper.Register(reversed);
    }
}