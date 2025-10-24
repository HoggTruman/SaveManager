using SaveManager.Exceptions;

namespace SaveManager.Models;

public interface IFilesystemItem
{
    /// <summary>
    /// The full filesystem path of the underlying file / directory.<br/>
    /// Setting a value does not affect the underlying items in the filesystem.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// The name of the underlying file / directory.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating if file / directory exists in the filesystem.
    /// </summary>
    public bool Exists { get; }

    /// <summary>
    /// The parent folder. It is only null for a folder representing a game's profiles directory or a file representing a game's savefile.
    /// </summary>
    public Folder? Parent { get; set; }

    /// <summary>
    /// Renames the underlying file / directory.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Rename(string newName);

    /// <summary>
    /// Deletes the underlying file / directory.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Delete();

    /// <summary>
    /// Moves the underlying file / directory into the folder provided.
    /// </summary>
    /// <param name="newParent"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void Move(Folder newParent);
}
