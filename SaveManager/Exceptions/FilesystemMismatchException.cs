namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown when a file or folder exists in the filesystem but not internally or internally but not in the filesystem.
/// </summary>
public class FilesystemMismatchException : Exception
{
    /// <summary>
    /// The absolute path of the item which triggered the exception.
    /// </summary>
    public string Location { get; }

    public FilesystemMismatchException(string location)
    { 
        Location = location; 
    }

    public FilesystemMismatchException(string location, string? message) : base(message)
    { 
        Location = location; 
    }

    public FilesystemMismatchException(string location, string? message, Exception? innerException) : base (message, innerException)
    { 
        Location = location; 
    } 
}
