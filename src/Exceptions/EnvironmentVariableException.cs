using SixLabors.ImageSharp;

namespace PigeonAPI.Exceptions;

/// <summary>
/// An exception for when an environment variable isn't present\
/// </summary>
public class EnvironmentVariableException : Exception
{
    public EnvironmentVariableException(string variableName) :
        base($"Required environment variable {variableName} was not present")
    {

    }
}