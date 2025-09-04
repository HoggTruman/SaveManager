namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown for file access related errors such as <see cref="UnauthorizedAccessException"/> and <see cref="System.Security.SecurityException"/>
/// </summary>
public class FileAccessException : Exception
{
    public FileAccessException() { }

    public FileAccessException(string? message) : base(message) { }

    public FileAccessException(string? message, Exception? innerException) : base (message, innerException) { }   
}
