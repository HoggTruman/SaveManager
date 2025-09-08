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
