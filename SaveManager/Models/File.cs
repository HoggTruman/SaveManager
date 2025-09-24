using SaveManager.Exceptions;
using System.IO;
using System.Security;

namespace SaveManager.Models;

public class File : IFilesystemItem
{
    private FileInfo _fileInfo;

    public string Location => _fileInfo.FullName;
    public string Name => _fileInfo.Name;
    public bool Exists => _fileInfo.Exists;

    /// <summary>
    /// The parent folder. It is only null for a folder representing a game's profiles directory.
    /// </summary>
    public Folder? Parent { get; set; }



    public File(string location, Folder? parent) : this(new FileInfo(location), parent) { }

    public File(FileInfo fileInfo, Folder? parent)
    {
        _fileInfo = fileInfo;
        Parent = parent;
    }

    /// <summary>
    /// Deletes the file in the filesystem and updates its parent.
    /// </summary>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void Delete()
    {
        if (Parent == null)
            throw new InvalidOperationException("A File's Parent must not be null.");

        if (!Exists)
            throw new FilesystemItemNotFoundException("The file you are trying to delete does not exist.");

        try
        {
            _fileInfo.Delete();
            Parent.Children = [..Parent.Children.Where(x => x != this)];
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException or IOException or SecurityException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }


    /// <summary>
    /// Updates the File's location for the internal filesystem representation.
    /// Does not actually affect any files or directories.
    /// </summary>
    /// <param name="newLocation"></param>
    /// <exception cref="FilesystemException"></exception>
    public void UpdateLocation(string newLocation)
    {
        try
        {
            _fileInfo = new(newLocation);
        }
        catch (Exception ex)
        {
            if (ex is PathTooLongException or ArgumentException or NotSupportedException or SecurityException or UnauthorizedAccessException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }
}
