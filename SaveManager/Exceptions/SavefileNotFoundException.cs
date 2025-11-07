namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown when a game's savefile does not exist in the filesystem.
/// </summary>
public class SavefileNotFoundException : Exception
{
    public SavefileNotFoundException() { }

    public SavefileNotFoundException(string? message) : base(message) { }

    public SavefileNotFoundException(string? message, Exception? innerException) : base (message, innerException) { }  
}
