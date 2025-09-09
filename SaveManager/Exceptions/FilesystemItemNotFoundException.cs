namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown when an internal file or folder does not exist in the filesystem.
/// </summary>
public class FilesystemItemNotFoundException : Exception
{
    public FilesystemItemNotFoundException() { }

    public FilesystemItemNotFoundException(string? message) : base(message) { }

    public FilesystemItemNotFoundException(string? message, Exception? innerException) : base (message, innerException) { }  
}
