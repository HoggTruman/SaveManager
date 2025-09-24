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
    /// Gets a value indicating if file / directory exists in the filesystem.
    /// </summary>
    public bool Exists { get; }

    /// <summary>
    /// The parent folder. It is only null for a folder representing a game's profiles directory.
    /// </summary>
    public Folder? Parent { get; set; }


    /// <summary>
    /// Deletes the underlying file / directory.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Delete();


    /// <summary>
    /// Updates the IFilesystemItem's location for the internal filesystem representation.
    /// Does not actually affect any files or directories.
    /// </summary>
    /// <param name="newLocation"></param>
    /// <exception cref="FilesystemException"></exception>
    public void UpdateLocation(string newLocation);
}
