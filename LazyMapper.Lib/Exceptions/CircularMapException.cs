namespace LazyMapper.Lib.Exceptions;

public class CircularMapException(string message) : Exception(message)
{
    public CircularMapException(Type source, Type destination)
        : this(
            $"Circular map detected. caused by mapping '{source.FullName}' to type '{destination.FullName}'.")
    {
    }
}