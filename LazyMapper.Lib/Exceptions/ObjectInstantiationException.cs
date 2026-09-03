namespace LazyMapper.Exceptions;

/// <summary>
/// Represents an exception thrown when an object cannot be instantiated during mapping.
/// </summary>
public class ObjectInstantiationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectInstantiationException"/> class.
    /// </summary>
    /// <param name="destinationType">The type that could not be instantiated.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public ObjectInstantiationException(Type destinationType, Exception innerException)
        : base($"Cannot create an instance of type '{destinationType.FullName}'. " +
               "Ensure the type has either a parameterless constructor or a constructor whose parameters match source properties.",
            innerException)
    {
    }
}