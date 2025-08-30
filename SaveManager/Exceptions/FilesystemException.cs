namespace SaveManager.Exceptions;

public class FilesystemException : Exception
{
    public FilesystemException() { }

    public FilesystemException(string? message) : base(message) { }

    public FilesystemException(string? message, Exception? innerException) : base (message, innerException) { } 
}
