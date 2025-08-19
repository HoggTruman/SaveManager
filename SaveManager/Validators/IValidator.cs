namespace SaveManager.Validators;

public interface IValidator
{
    /// <summary>
    /// Returns true if the argument is valid.
    /// If false, the reason can be accessed via the Message property.
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    bool IsValid(string field);

    /// <summary>
    /// Describes the result of the previous IsValid call.
    /// </summary>
    string Message { get; }
}
