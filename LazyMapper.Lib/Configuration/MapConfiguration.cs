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

    public void ReverseMap()
    {
        var reversed = _profile.Reverse();
        _mapper.Register(reversed);
    }
}