namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown for general failure to perform filesystem actions.
/// Usually the result of the filesystem being inconsistent with its internal representation.
/// </summary>
public class FilesystemException : Exception
{
    public FilesystemException() { }

    public FilesystemException(string? message) : base(message) { }

    public FilesystemException(string? message, Exception? innerException) : base (message, innerException) { } 
}
