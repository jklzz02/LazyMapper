namespace LazyMapper.Exceptions;

/// <summary>
/// Represents an exception thrown when duplicate profile configurations are detected during mapping registration.
/// </summary>
public class DuplicateProfilesException(string message) : Exception(message)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateProfilesException"/> class.
    /// </summary>
    /// <param name="profile">The duplicated profile.</param>
    public DuplicateProfilesException(Type profile)
        : this($"Cannot register multiple profiles of type '{profile.FullName}'")
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateProfilesException"/> class.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="destination">The destination type.</param>
    public DuplicateProfilesException(Type source, Type destination)
        : this($"Cannot register multiple profiles with source '{source.FullName}' and destination '{destination.FullName}'")
    {
    }
}