namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown for general failure to perform filesystem actions.
/// </summary>
public class FilesystemException : Exception
{
    public FilesystemException() { }

    public FilesystemException(string? message) : base(message) { }

    public FilesystemException(string? message, Exception? innerException) : base (message, innerException) { } 
}
