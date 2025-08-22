namespace SaveManager.Exceptions;

public class AppdataException : Exception
{
    public AppdataException() { }

    public AppdataException(string? message) : base(message) { }

    public AppdataException(string? message, Exception? innerException) : base (message, innerException) { }    
}
