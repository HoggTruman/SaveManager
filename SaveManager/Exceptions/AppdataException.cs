namespace SaveManager.Exceptions;

/// <summary>
/// An exception thrown when failing to interact with the appdata file.
/// </summary>
public class AppdataException : Exception
{
    public AppdataException() { }

    public AppdataException(string? message) : base(message) { }

    public AppdataException(string? message, Exception? innerException) : base (message, innerException) { }    
}
