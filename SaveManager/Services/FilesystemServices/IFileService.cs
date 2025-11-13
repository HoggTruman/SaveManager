using System.IO;
using SaveManager.Exceptions;

namespace SaveManager.Services.FilesystemServices;

public interface IFileService
{
    /// <inheritdoc cref="File.Exists(string?)"/>
    public bool Exists(string location);

    /// <summary>
    /// Copies an existing file to another location. 
    /// </summary>
    /// <param name="sourceLocation">The full path of the file to be copied.</param>
    /// <param name="destinationLocation">The full path to copy the file to.</param>
    /// <param name="overwrite">Whether an existing file at the destination should be overwritten.</param>
    /// <exception cref="FilesystemException"></exception>
    public void Copy(string sourceLocation, string destinationLocation, bool overwrite=false);

    /// <summary>
    /// Moves a file to a new location, providing the option to rename it.
    /// </summary>
    /// <param name="sourceLocation">The full path of the file to be moved.</param>
    /// <param name="destinationLocation">The full path to move the file to.</param>
    /// <exception cref="FilesystemException"></exception>
    public void Move(string sourceLocation, string destinationLocation);

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="location">The full path of the file to be deleted.</param>
    /// <exception cref="FilesystemException"></exception>
    public void Delete(string location);
}
