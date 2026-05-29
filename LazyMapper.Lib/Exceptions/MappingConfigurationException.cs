namespace LazyMapper.Exceptions;

/// <summary>
/// Represents an exception thrown when a mapping configuration is invalid.
/// </summary>
/// <param name="message">The exception message.</param>
public class MappingConfigurationException(string message) : Exception(message)
{
}
