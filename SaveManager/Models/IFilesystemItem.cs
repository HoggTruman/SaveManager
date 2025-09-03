using SaveManager.Exceptions;

namespace SaveManager.Models;

public interface IFilesystemItem
{
    /// <summary>
    /// The full filesystem path of the underlying file / directory.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// The name of the underlying file / directory.
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// Updates the IFilesystemItem's location for the internal filesystem representation.
    /// Does not actually affect any files or directories.
    /// </summary>
    /// <param name="newLocation"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void UpdateLocation(string newLocation);
}
