namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown when an internal file or folder does not exist in the filesystem.
/// </summary>
public class FilesystemItemNotFoundException : Exception
{
    /// <summary>
    /// The absolute path of the item which triggered the exception.
    /// </summary>
    public string Location { get; }

    public FilesystemItemNotFoundException(string location)
    { 
        Location = location; 
    }

    public FilesystemItemNotFoundException(string location, string? message) : base(message)
    { 
        Location = location; 
    }

    public FilesystemItemNotFoundException(string location, string? message, Exception? innerException) : base (message, innerException)
    { 
        Location = location; 
    } 
}
