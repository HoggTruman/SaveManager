using SaveManager.Exceptions;
using System.IO;
using System.Security;

namespace SaveManager.Models;

public class File : IFilesystemItem
{
    private FileInfo _fileInfo;

    public string Location => _fileInfo.FullName;
    public string Name => _fileInfo.Name;

    public File(string location) : this(new FileInfo(location)) { }

    public File(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }


    /// <summary>
    /// Updates the File's location for the internal filesystem representation.
    /// Does not actually affect any files or directories.
    /// </summary>
    /// <param name="newLocation"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void UpdateLocation(string newLocation)
    {
        try
        {
            _fileInfo = new(newLocation);
        }
        catch (Exception ex)
        {
            if (ex is PathTooLongException)
                throw new ValidationException($"Filepath is too long");

            if (ex is ArgumentException or NotSupportedException)
                throw new ValidationException($"Filepath contains invalid characters: {newLocation}");

            if (ex is SecurityException or UnauthorizedAccessException)
                throw new FilesystemException($"Could not access file: {newLocation}");

            throw;
        }
    }
}
