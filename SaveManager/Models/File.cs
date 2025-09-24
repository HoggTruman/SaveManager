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
    /// Renames the file in the filesystem.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void Rename(string newName)
    {
        if (Parent == null)
            throw new InvalidOperationException("A File's Parent must not be null.");

        ValidateFileName(newName, Parent.Children);
        
        if (!Exists)
            throw new FilesystemItemNotFoundException("The file you are trying to rename does not exist.");

        try
        {
            string newLocation = Path.Join(Parent.Location, newName);
            _fileInfo.MoveTo(newLocation);
            Parent.SortChildren();
        }
        catch(Exception ex)
        {
            if (ex is IOException or ArgumentException or SecurityException or UnauthorizedAccessException or FileNotFoundException or
                DirectoryNotFoundException or PathTooLongException or NotSupportedException)
                throw new FilesystemException(ex.Message, ex);
                
            throw;
        }
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


    /// <summary>
    /// Validates file name. Throws a <see cref="ValidationException"/> if not valid.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ValidationException"></exception>
    internal static void ValidateFileName(string name, IEnumerable<IFilesystemItem> siblings)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("File name can not be empty or whitespace.");

        if (Path.GetInvalidFileNameChars().Any(name.Contains))
            throw new ValidationException($"File name can not contain any of the following: {new(Path.GetInvalidFileNameChars())}");

        if (siblings.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
            throw new ValidationException("A file already exists with this name.");
    }
}
